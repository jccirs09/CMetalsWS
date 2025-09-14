using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace CMetalsWS.Services
{
    public class ConfigGaugeWeightResolver : IGaugeWeightResolver
    {
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, decimal> _gaugeMap;

        public ConfigGaugeWeightResolver(IConfiguration configuration)
        {
            _configuration = configuration;
            // Binds the configuration section "GaugeWeightMap" to a temporary dictionary,
            // then creates the final dictionary with a case-insensitive key comparer.
            var tempMap = _configuration.GetSection("GaugeWeightMap").Get<Dictionary<string, decimal>>()
                          ?? new Dictionary<string, decimal>();
            _gaugeMap = new Dictionary<string, decimal>(tempMap, System.StringComparer.OrdinalIgnoreCase);
        }

        public decimal GetWeightPerSquareFoot(string gauge)
        {
            // The gauge from the WorkOrderItem might be complex, e.g., "18GA-Galvanized".
            // We need to find a key in our map that is a substring of the input gauge.
            // This is a simple approach. A more robust solution might use regex or more structured data.
            if (string.IsNullOrEmpty(gauge))
            {
                return 0;
            }

            // Find the best matching key from our map. E.g., "18GA" in "18GA-Galvanized".
            var matchingKey = _gaugeMap.Keys.FirstOrDefault(key => gauge.Contains(key, System.StringComparison.OrdinalIgnoreCase));

            if (matchingKey != null && _gaugeMap.TryGetValue(matchingKey, out var weight))
            {
                return weight;
            }

            return 0; // Return 0 if no matching gauge is found in the configuration.
        }
    }
}
