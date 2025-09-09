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

        private static readonly List<string> MetroVancouverCities = new() { "Vancouver", "Surrey", "Richmond", "Burnaby", "Delta", "Langley", "Coquitlam", "Port Coquitlam", "Port Moody", "New Westminster", "North Vancouver", "West Vancouver", "Maple Ridge", "Pitt Meadows", "White Rock" };
        private static readonly List<string> VancouverIslandCities = new() { "Victoria", "Nanaimo", "Duncan", "Parksville", "Courtenay", "Comox", "Campbell River", "Port Alberni" };
        private static readonly List<string> OkanaganCities = new() { "Okanagan Falls", "Penticton", "Kelowna", "Vernon", "Salmon Arm", "Kamloops" };

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

            if (customer.Latitude.HasValue && customer.Longitude.HasValue)
            {
                customer.DestinationRegionCategory = ComputeDestinationRegionCategory(customer);
                customer.DestinationGroupCategory = await ComputeDestinationGroupCategoryAsync(customer);
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

            var allCities = MetroVancouverCities.Concat(VancouverIslandCities).Concat(OkanaganCities);
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
                if (MetroVancouverCities.Contains(candidate.City, StringComparer.OrdinalIgnoreCase)) score += 10;
                else if (VancouverIslandCities.Contains(candidate.City, StringComparer.OrdinalIgnoreCase)) score += 10;
                else if (OkanaganCities.Contains(candidate.City, StringComparer.OrdinalIgnoreCase)) score += 10;
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
            originalCustomer.FullAddress = enrichedCustomer.FullAddress;
            return originalCustomer;
        }

        private DestinationRegionCategory ComputeDestinationRegionCategory(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.City)) return DestinationRegionCategory.OUT_OF_TOWN;

            if (MetroVancouverCities.Contains(customer.City, StringComparer.OrdinalIgnoreCase)) return DestinationRegionCategory.LOCAL;
            if (VancouverIslandCities.Contains(customer.City, StringComparer.OrdinalIgnoreCase)) return DestinationRegionCategory.ISLAND;
            if (OkanaganCities.Contains(customer.City, StringComparer.OrdinalIgnoreCase)) return DestinationRegionCategory.OKANAGAN;

            return DestinationRegionCategory.OUT_OF_TOWN;
        }

        private async Task<string> ComputeDestinationGroupCategoryAsync(Customer customer)
        {
            if (customer.DestinationRegionCategory != DestinationRegionCategory.LOCAL)
            {
                return customer.City ?? "UNKNOWN";
            }

            using var db = _dbContextFactory.CreateDbContext();
            var centroids = await db.CityCentroids.Where(c => c.Province == "BC").ToListAsync();

            if (!centroids.Any() || !customer.Latitude.HasValue || !customer.Longitude.HasValue)
            {
                return customer.City ?? "UNKNOWN";
            }

            var closestCentroid = centroids
                .Select(c => new { Centroid = c, Distance = GetDistance(customer.Latitude.Value, customer.Longitude.Value, c.Latitude, c.Longitude) })
                .OrderBy(x => x.Distance)
                .FirstOrDefault()?.Centroid;

            if (closestCentroid == null)
            {
                return customer.City ?? "UNKNOWN";
            }

            var centralPointLat = 49.2609m;
            var centralPointLon = -123.1139m;
            var direction = (customer.Latitude > centralPointLat ? "N" : "S") + (customer.Longitude > centralPointLon ? "E" : "W");

            return $"{direction}_{customer.City}".ToUpper();
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
    }
}
