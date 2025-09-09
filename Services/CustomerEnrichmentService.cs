using CMetalsWS.Data;
using CMetalsWS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class CustomerEnrichmentService : ICustomerEnrichmentService
    {
        private readonly IGooglePlacesService _googlePlacesService;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<CustomerEnrichmentService> _logger;
        private static readonly SemaphoreSlim _googleApiSemaphore = new(5);

        static readonly HashSet<string> MetroVancouver = new(StringComparer.OrdinalIgnoreCase)
        {
            "Vancouver","Burnaby","Richmond","Surrey","Delta","Langley","Langley Township",
            "White Rock","New Westminster","Coquitlam","Port Coquitlam","Port Moody",
            "North Vancouver","West Vancouver","Maple Ridge","Pitt Meadows"
        };

        static readonly HashSet<string> VancouverIsland = new(StringComparer.OrdinalIgnoreCase)
        {
            "Victoria","Saanich","Langford","Colwood","Esquimalt","Sidney","Sooke",
            "Nanaimo","Parksville","Qualicum Beach","Duncan","Ladysmith",
            "Courtenay","Comox","Campbell River","Port Alberni"
        };

        static readonly HashSet<string> OkanaganCorridor = new(StringComparer.OrdinalIgnoreCase)
        {
            "Okanagan Falls","Penticton","Summerland","Peachland","West Kelowna",
            "Kelowna","Lake Country","Vernon","Coldstream","Armstrong",
            "Enderby","Salmon Arm","Sicamous","Kamloops"
        };

        public CustomerEnrichmentService(IGooglePlacesService googlePlacesService, IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<CustomerEnrichmentService> logger)
        {
            _googlePlacesService = googlePlacesService;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<CustomerEnrichmentResult> EnrichAndCategorizeCustomerAsync(Customer customer, string? address)
        {
            var result = new CustomerEnrichmentResult();
            var cleanedName = CleanCustomerName(customer.CustomerName);
            var queryCandidates = BuildQueryCandidates(cleanedName, address);

            var scoredCandidates = new List<(Customer candidate, double score)>();

            foreach (var query in queryCandidates)
            {
                await _googleApiSemaphore.WaitAsync();
                try
                {
                    var candidates = await _googlePlacesService.SearchPlacesAsync(query);
                    foreach (var candidate in candidates)
                    {
                        var score = ScoreCandidate(customer, candidate);
                        scoredCandidates.Add((candidate, score));
                        if (score >= 80)
                        {
                            // Early exit if we have a very strong match
                            goto FoundBestCandidate;
                        }
                    }
                }
                finally
                {
                    _googleApiSemaphore.Release();
                }
            }

            FoundBestCandidate:
            var bestCandidate = scoredCandidates.OrderByDescending(c => c.score).FirstOrDefault().candidate;

            if (bestCandidate != null && scoredCandidates.First().score >= 70)
            {
                var enrichedCustomer = await _googlePlacesService.GetPlaceDetailsAsync(bestCandidate.PlaceId!);
                if (enrichedCustomer != null)
                {
                    customer = MapEnrichedData(customer, enrichedCustomer);
                    result.EnrichedCustomer = customer;
                }
            }
            else
            {
                result.RequiresManualSelection = true;
                result.Candidates = scoredCandidates.OrderByDescending(c => c.score).Take(3).Select(c => c.candidate).ToList();
                _logger.LogWarning("Weak match for customer {CustomerCode}. Top 3 candidates: {Candidates}", customer.CustomerCode, string.Join(", ", result.Candidates.Select(c => c.CustomerName)));
            }

            if (customer.Latitude.HasValue && customer.Longitude.HasValue && customer.City != null && customer.Province != null && customer.Country != null)
            {
                customer.DestinationRegionCategory = ComputeRegion(customer.City, customer.Province, customer.Country);
                customer.DestinationGroupCategory = ComputeGroup(customer.DestinationRegionCategory, customer.City, (double?)customer.Latitude, (double?)customer.Longitude);
            }

            return result;
        }

        private string CleanCustomerName(string name)
        {
            return Regex.Replace(name, @"\b(ltd|inc|corp|plant\s*\d*)\b", "", RegexOptions.IgnoreCase).Trim();
        }

        private List<string> BuildQueryCandidates(string cleanedName, string? address)
        {
            var candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(address))
            {
                candidates.Add($"{cleanedName}, {address}");
            }
            candidates.Add($"{cleanedName}, British Columbia, Canada");
            candidates.Add($"{cleanedName}, Canada");

            var allCities = MetroVancouver.Concat(VancouverIsland).Concat(OkanaganCorridor);
            foreach (var city in allCities)
            {
                candidates.Add($"{cleanedName}, {city}, BC, Canada");
            }
            return candidates.Distinct().ToList();
        }

        private double ScoreCandidate(Customer originalCustomer, Customer candidate)
        {
            double score = 0;

            // Token overlap scoring
            var originalTokens = new HashSet<string>(CleanCustomerName(originalCustomer.CustomerName).ToLower().Split(' '));
            var candidateTokens = new HashSet<string>(CleanCustomerName(candidate.CustomerName).ToLower().Split(' '));
            var overlap = originalTokens.Intersect(candidateTokens).Count();
            var total = originalTokens.Union(candidateTokens).Count();
            if (total > 0)
            {
                score += (double)overlap / total * 70; // 70% of score is based on name overlap
            }

            // Bonus for city match
            if (!string.IsNullOrWhiteSpace(candidate.City))
            {
                if (MetroVancouver.Contains(candidate.City)) score += 10;
                else if (VancouverIsland.Contains(candidate.City)) score += 10;
                else if (OkanaganCorridor.Contains(candidate.City)) score += 10;
            }

            // Penalty for irrelevant types
            if (candidate.CustomTags != null)
            {
                if (candidate.CustomTags.Contains("park", StringComparison.OrdinalIgnoreCase) || candidate.CustomTags.Contains("route", StringComparison.OrdinalIgnoreCase))
                {
                    score -= 20;
                }
            }

            return Math.Max(0, score); // Ensure score is not negative
        }

        private Customer MapEnrichedData(Customer originalCustomer, Customer enrichedCustomer)
        {
            originalCustomer.PlaceId = enrichedCustomer.PlaceId;
            originalCustomer.Latitude = enrichedCustomer.Latitude;
            originalCustomer.Longitude = enrichedCustomer.Longitude;
            originalCustomer.Street1 = enrichedCustomer.Street1;
            originalCustomer.Street2 = enrichedCustomer.Street2;
            originalCustomer.City = enrichedCustomer.City;
            originalCustomer.Province = enrichedCustomer.Province;
            originalCustomer.PostalCode = enrichedCustomer.PostalCode;
            originalCustomer.Country = enrichedCustomer.Country;
            originalCustomer.Address = enrichedCustomer.Address;
            return originalCustomer;
        }

        public static DestinationRegionCategory ComputeRegion(string city, string province, string country)
        {
            if (!"Canada".Equals(country, StringComparison.OrdinalIgnoreCase)) return DestinationRegionCategory.OUT_OF_TOWN;
            if (!"BC".Equals(province, StringComparison.OrdinalIgnoreCase))    return DestinationRegionCategory.OUT_OF_TOWN;

            if (MetroVancouver.Contains(city)) return DestinationRegionCategory.LOCAL;
            if (VancouverIsland.Contains(city)) return DestinationRegionCategory.ISLAND;
            if (OkanaganCorridor.Contains(city)) return DestinationRegionCategory.OKANAGAN;

            return DestinationRegionCategory.OUT_OF_TOWN; // rest of BC
        }

        public static string ComputeGroup(DestinationRegionCategory region, string city, double? lat, double? lng)
        {
            if (region == DestinationRegionCategory.LOCAL && lat.HasValue && lng.HasValue)
            {
                var quad = ToCardinal(GetQuadrant(city, lat.Value, lng.Value));
                return $"{quad} {city}".ToUpperInvariant();
            }
            return city?.ToUpperInvariant() ?? "UNKNOWN";
        }

        private double GetDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var r = 6371; // Radius of earth in km
            var dLat = (double)(lat2 - lat1) * (Math.PI / 180);
            var dLon = (double)(lon2 - lon1) * (Math.PI / 180);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos((double)lat1 * (Math.PI / 180)) * Math.Cos((double)lat2 * (Math.PI / 180)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }

        public record LatLng(double Lat, double Lng);

        static readonly Dictionary<string, LatLng> CityCentroids = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Surrey"] = new(49.1044, -122.8011),
            ["Vancouver"] = new(49.2827, -123.1207),
            ["Richmond"] = new(49.1666, -123.1336),
        };

        public static string GetQuadrant(string city, double lat, double lng)
        {
            if (!CityCentroids.TryGetValue(city, out var c)) return "CENTER"; // fallback

            var dLat = lat - c.Lat;
            var dLng = lng - c.Lng;

            if (Math.Abs(dLat) < 0.01 && Math.Abs(dLng) < 0.01) return "CENTER";

            var north = dLat > 0;
            var east  = dLng > 0;

            if (north && east)  return "NORTHEAST";
            if (north && !east) return "NORTHWEST";
            if (!north && east) return "SOUTHEAST";
            return "SOUTHWEST";
        }

        public static string ToCardinal(string quadrant)
        {
            return quadrant switch
            {
                "NORTHEAST" => "NORTH",
                "NORTHWEST" => "NORTH",
                "SOUTHEAST" => "SOUTH",
                "SOUTHWEST" => "SOUTH",
                _ => "CENTER"
            };
        }
    }
}
