using System;
using System.Collections.Generic;
using System.Linq;

namespace CMetalsWS.Services
{
    public class RegionAssignmentService
    {
        private readonly HashSet<string> _localCities;
        private readonly HashSet<string> _islandCities;
        private readonly HashSet<string> _okanaganCities;

        public RegionAssignmentService()
        {
            // Using HashSet for efficient lookups
            _localCities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Vancouver", "Burnaby", "New Westminster", "Coquitlam", "Port Coquitlam", "Port Moody",
                "Anmore", "Belcarra", "Surrey", "Richmond", "Delta", "White Rock", "North Vancouver",
                "West Vancouver", "Lions Bay", "Pitt Meadows", "Maple Ridge", "Langley", "Abbotsford",
                "Mission", "Chilliwack", "Hope", "Squamish", "Whistler", "Pemberton", "Gibsons", "Sechelt", "Rosedale"
            };

            _islandCities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Victoria", "Saanich", "Esquimalt", "Oak Bay", "View Royal", "Colwood", "Langford",
                "Metchosin", "Sooke", "Sidney", "North Saanich", "Central Saanich", "Nanaimo",
                "Ladysmith", "Parksville", "Qualicum Beach", "Port Alberni", "Ucluelet", "Tofino",
                "Courtenay", "Comox", "Cumberland", "Campbell River", "Gold River", "Tahsis", "Sayward",
                "Port McNeill", "Port Hardy", "Port Alice", "Zeballos", "Lake Cowichan", "Duncan"
            };

            _okanaganCities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Kelowna", "West Kelowna", "Vernon", "Penticton", "Summerland", "Peachland", "Lake Country",
                "Armstrong", "Enderby", "Lumby", "Coldstream", "Oliver", "Osoyoos", "Keremeos", "Princeton",
                "Sicamous", "Salmon Arm", "Revelstoke", "Kamloops"
            };
        }

        public string GetRegionName(string? city, string? province)
        {
            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(province))
            {
                return "Out of Town";
            }

            if (!province.Equals("BC", StringComparison.OrdinalIgnoreCase))
            {
                return "Out of Town";
            }

            if (_localCities.Contains(city))
            {
                return "Local";
            }

            if (_islandCities.Contains(city))
            {
                return "Island";
            }

            if (_okanaganCities.Contains(city))
            {
                return "Okanagan";
            }

            return "Out of Town";
        }
    }
}