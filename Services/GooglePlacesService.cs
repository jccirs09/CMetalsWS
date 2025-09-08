using CMetalsWS.Data;
using CMetalsWS.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class GooglePlacesService : IGooglePlacesService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GooglePlacesService> _logger;
        private readonly string? _apiKey;

        public GooglePlacesService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GooglePlacesService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _apiKey = configuration["GooglePlaces:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Goog-FieldMask", "places.name,places.displayName,places.formattedAddress,places.location,places.types");
        }

        public async Task<List<Customer>> SearchPlacesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("Google Places API key is not configured. Skipping search.");
                return new List<Customer>();
            }

            var request = new GooglePlacesTextSearchRequest
            {
                textQuery = query,
                regionCode = "CA",
                locationBias = new LocationBias
                {
                    rectangle = new Rectangle
                    {
                        low = new LatLng { latitude = 49.0, longitude = -123.5 },
                        high = new LatLng { latitude = 49.5, longitude = -122.2 }
                    }
                },
                maxResultCount = 5
            };

            var response = await _httpClient.PostAsJsonAsync("https://places.googleapis.com/v1/places:searchText", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google Places Text Search API request failed with status code {StatusCode}", response.StatusCode);
                return new List<Customer>();
            }

            var content = await response.Content.ReadFromJsonAsync<PlacesTextSearchResponseV2>();
            return content?.places?.Select(p => new Customer
            {
                CustomerName = p.displayName?.text ?? p.name,
                FullAddress = p.formattedAddress,
                Latitude = (decimal?)p.location?.latitude,
                Longitude = (decimal?)p.location?.longitude,
                PlaceId = p.name?.Split('/').Last(),
                CustomTags = string.Join(",", p.types ?? new List<string>())
            }).ToList() ?? new List<Customer>();
        }

        public async Task<Customer?> GetPlaceDetailsAsync(string placeId)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(placeId))
            {
                return null;
            }

            var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&key={_apiKey}&fields=address_components,formatted_address";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google Places Details API request failed with status code {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadFromJsonAsync<PlaceDetailsResponse>();
            if (content?.result == null) return null;

            var customer = new Customer();
            MapPlaceDetailsToCustomer(content.result, customer);
            return customer;
        }

        private void MapPlaceDetailsToCustomer(PlaceDetailsResult details, Customer customer)
        {
            customer.Street1 = $"{details.address_components?.FirstOrDefault(c => c.types != null && c.types.Contains("street_number"))?.long_name} {details.address_components?.FirstOrDefault(c => c.types != null && c.types.Contains("route"))?.long_name}".Trim();
            customer.City = details.address_components?.FirstOrDefault(c => c.types != null && c.types.Contains("locality"))?.long_name;
            customer.Province = details.address_components?.FirstOrDefault(c => c.types != null && c.types.Contains("administrative_area_level_1"))?.short_name;
            customer.PostalCode = details.address_components?.FirstOrDefault(c => c.types != null && c.types.Contains("postal_code"))?.long_name;
            customer.Country = details.address_components?.FirstOrDefault(c => c.types != null && c.types.Contains("country"))?.long_name;
            customer.FullAddress = details.formatted_address;
        }
    }

    public class PlacesTextSearchResponseV2
    {
        public List<PlaceV2>? places { get; set; }
    }

    public class PlaceV2
    {
        public string? name { get; set; }
        public DisplayName? displayName { get; set; }
        public string? formattedAddress { get; set; }
        public LocationV2? location { get; set; }
        public List<string>? types { get; set; }
    }

    public class DisplayName
    {
        public string? text { get; set; }
        public string? languageCode { get; set; }
    }

    public class LocationV2
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class PlaceDetailsResponse
    {
        public PlaceDetailsResult? result { get; set; }
    }

    public class PlaceDetailsResult
    {
        public AddressComponent[]? address_components { get; set; }
        public string? formatted_address { get; set; }
    }

    public class AddressComponent
    {
        public string? long_name { get; set; }
        public string? short_name { get; set; }
        public string[]? types { get; set; }
    }
}
