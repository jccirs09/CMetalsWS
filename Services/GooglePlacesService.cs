using CMetalsWS.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

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
        }

        public async Task<Customer> EnrichCustomerAddressAsync(Customer customer, string address)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("Google Places API key is not configured. Skipping address enrichment.");
                return customer;
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                _logger.LogWarning("Address is empty for customer {CustomerCode}. Skipping address enrichment.", customer.CustomerCode);
                return customer;
            }

            try
            {
                // 1. Text Search to find Place ID
                var placeId = await FindPlaceIdAsync(address);
                if (string.IsNullOrWhiteSpace(placeId))
                {
                    _logger.LogWarning("Could not find Place ID for address: {Address}", address);
                    return customer;
                }

                // 2. Place Details to get address components and geometry
                var placeDetails = await GetPlaceDetailsAsync(placeId);
                if (placeDetails?.result == null)
                {
                    _logger.LogWarning("Could not get Place Details for Place ID: {PlaceId}", placeId);
                    return customer;
                }

                // 3. Map details to customer object
                MapPlaceDetailsToCustomer(placeDetails.result, customer);

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching address for customer {CustomerCode}", customer.CustomerCode);
                return customer; // Return original customer on error
            }
        }

        private async Task<string?> FindPlaceIdAsync(string address)
        {
            var encodedAddress = HttpUtility.UrlEncode(address);
            var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={encodedAddress}&key={_apiKey}&fields=place_id";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google Places Text Search API request failed with status code {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadFromJsonAsync<PlacesTextSearchResponse>();
            return content?.results?.FirstOrDefault()?.place_id;
        }

        private async Task<PlaceDetailsResponse?> GetPlaceDetailsAsync(string placeId)
        {
            var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&key={_apiKey}&fields=address_components,geometry,place_id";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google Places Details API request failed with status code {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PlaceDetailsResponse>();
        }

        private void MapPlaceDetailsToCustomer(PlaceDetailsResult details, Customer customer)
        {
            customer.PlaceId = details.place_id;
            customer.Latitude = details.geometry?.location?.lat;
            customer.Longitude = details.geometry?.location?.lng;

            var streetNumber = details.address_components?.FirstOrDefault(c => c.types.Contains("street_number"))?.long_name;
            var route = details.address_components?.FirstOrDefault(c => c.types.Contains("route"))?.long_name;

            customer.Street1 = $"{streetNumber} {route}".Trim();
            customer.City = details.address_components?.FirstOrDefault(c => c.types.Contains("locality"))?.long_name;
            customer.Province = details.address_components?.FirstOrDefault(c => c.types.Contains("administrative_area_level_1"))?.short_name;
            customer.PostalCode = details.address_components?.FirstOrDefault(c => c.types.Contains("postal_code"))?.long_name;
            customer.Country = details.address_components?.FirstOrDefault(c => c.types.Contains("country"))?.long_name;

            customer.FullAddress = $"{customer.Street1}, {customer.City}, {customer.Province} {customer.PostalCode}, {customer.Country}".Trim();
        }
    }

    // --- DTOs for Google Places API responses ---

    public class PlacesTextSearchResponse
    {
        public PlaceSearchResult[]? results { get; set; }
        public string? status { get; set; }
    }

    public class PlaceSearchResult
    {
        public string? place_id { get; set; }
    }

    public class PlaceDetailsResponse
    {
        public PlaceDetailsResult? result { get; set; }
        public string? status { get; set; }
    }

    public class PlaceDetailsResult
    {
        public AddressComponent[]? address_components { get; set; }
        public Geometry? geometry { get; set; }
        public string? place_id { get; set; }
    }

    public class AddressComponent
    {
        public string? long_name { get; set; }
        public string? short_name { get; set; }
        public string[]? types { get; set; }
    }

    public class Geometry
    {
        public Location? location { get; set; }
    }

    public class Location
    {
        public decimal lat { get; set; }
        public decimal lng { get; set; }
    }
}
