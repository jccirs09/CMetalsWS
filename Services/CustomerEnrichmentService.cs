using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class CustomerEnrichmentService : ICustomerEnrichmentService
    {
        private readonly IGooglePlacesService _googlePlacesService;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<CustomerEnrichmentService> _logger;

        // Bounding box for Metro Vancouver (very rough)
        private const decimal VAN_LAT_MIN = 49.0m;
        private const decimal VAN_LAT_MAX = 49.4m;
        private const decimal VAN_LON_MIN = -123.3m;
        private const decimal VAN_LON_MAX = -122.5m;

        // Bounding box for Vancouver Island (very rough)
        private const decimal VI_LAT_MIN = 48.3m;
        private const decimal VI_LAT_MAX = 50.8m;
        private const decimal VI_LON_MIN = -128.5m;
        private const decimal VI_LON_MAX = -123.2m;

        // Bounding box for Okanagan (very rough)
        private const decimal OK_LAT_MIN = 49.0m;
        private const decimal OK_LAT_MAX = 50.7m;
        private const decimal OK_LON_MIN = -120.0m;
        private const decimal OK_LON_MAX = -119.2m;

        public CustomerEnrichmentService(IGooglePlacesService googlePlacesService, IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<CustomerEnrichmentService> logger)
        {
            _googlePlacesService = googlePlacesService;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Customer> EnrichAndCategorizeCustomerAsync(Customer customer, string address)
        {
            // 1. Enrich address using Google Places API
            customer = await _googlePlacesService.EnrichCustomerAddressAsync(customer, address);

            if (customer.Latitude.HasValue && customer.Longitude.HasValue)
            {
                // 2. Compute DestinationRegionCategory
                customer.DestinationRegionCategory = ComputeDestinationRegionCategory(customer.Latitude.Value, customer.Longitude.Value);

                // 3. Compute DestinationGroupCategory
                customer.DestinationGroupCategory = await ComputeDestinationGroupCategoryAsync(customer);
            }
            else
            {
                _logger.LogWarning("Cannot categorize customer {CustomerCode} without coordinates.", customer.CustomerCode);
            }

            return customer;
        }

        private DestinationRegionCategory ComputeDestinationRegionCategory(decimal lat, decimal lon)
        {
            if (lat >= VAN_LAT_MIN && lat <= VAN_LAT_MAX && lon >= VAN_LON_MIN && lon <= VAN_LON_MAX)
            {
                return DestinationRegionCategory.LOCAL;
            }
            if (lat >= VI_LAT_MIN && lat <= VI_LAT_MAX && lon >= VI_LON_MIN && lon <= VI_LON_MAX)
            {
                return DestinationRegionCategory.ISLAND;
            }
            if (lat >= OK_LAT_MIN && lat <= OK_LAT_MAX && lon >= OK_LON_MIN && lon <= OK_LON_MAX)
            {
                return DestinationRegionCategory.OKANAGAN;
            }
            return DestinationRegionCategory.OUT_OF_TOWN;
        }

        private async Task<string> ComputeDestinationGroupCategoryAsync(Customer customer)
        {
            if (customer.DestinationRegionCategory != DestinationRegionCategory.LOCAL)
            {
                return customer.City ?? "UNKNOWN";
            }

            // For LOCAL, find closest city centroid and determine direction
            using var db = _dbContextFactory.CreateDbContext();
            var centroids = await db.CityCentroids.Where(c => c.Province == "BC").ToListAsync();

            if (!centroids.Any() || !customer.Latitude.HasValue || !customer.Longitude.HasValue)
            {
                return customer.City ?? "UNKNOWN";
            }

            CityCentroid? closestCentroid = null;
            double minDistance = double.MaxValue;

            foreach (var centroid in centroids)
            {
                var distance = GetDistance(customer.Latitude.Value, customer.Longitude.Value, centroid.Latitude, centroid.Longitude);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCentroid = centroid;
                }
            }

            if (closestCentroid == null)
            {
                return customer.City ?? "UNKNOWN";
            }

            // Simplified direction logic relative to a central point in Vancouver (e.g., City Hall)
            var vancouverCityHallLat = 49.2609m;
            var vancouverCityHallLon = -123.1139m;

            var direction = "";
            if (customer.Latitude > vancouverCityHallLat) direction += "N";
            else direction += "S";

            if (customer.Longitude > vancouverCityHallLon) direction += "E";
            else direction += "W";

            return $"{direction}_{closestCentroid.City}".ToUpper();
        }

        private double GetDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var r = 6371; // Radius of the earth in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = r * c; // Distance in km
            return d;
        }

        private double ToRadians(decimal angle)
        {
            return (double)angle * (Math.PI / 180);
        }
    }
}
