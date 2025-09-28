using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Data.Seed
{
    public class DestinationDataSeeder
    {
        private readonly ApplicationDbContext _db;

        public DestinationDataSeeder(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            await SeedRegionsAsync();
            await SeedGroupsAsync();
        }

        private async Task SeedRegionsAsync()
        {
            if (await _db.DestinationRegions.AnyAsync()) return;

            var regions = new List<DestinationRegion>
            {
                new DestinationRegion { Name = "Local" },
                new DestinationRegion { Name = "Island" },
                new DestinationRegion { Name = "Okanagan" },
                new DestinationRegion { Name = "Out of Town" }
            };

            _db.DestinationRegions.AddRange(regions);
            await _db.SaveChangesAsync();
        }

        private async Task SeedGroupsAsync()
        {
            if (await _db.DestinationGroups.AnyAsync()) return;

            var allCities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Using comprehensive lists based on search results
            var bcCities = new List<string>
            {
                // Lower Mainland / Local
                "Vancouver", "Burnaby", "New Westminster", "Coquitlam", "Port Coquitlam", "Port Moody",
                "Anmore", "Belcarra", "Surrey", "Richmond", "Delta", "White Rock", "North Vancouver",
                "West Vancouver", "Lions Bay", "Pitt Meadows", "Maple Ridge", "Langley", "Abbotsford",
                "Mission", "Chilliwack", "Hope", "Squamish", "Whistler", "Pemberton", "Gibsons", "Sechelt", "Rosedale",

                // Vancouver Island / Island
                "Victoria", "Saanich", "Esquimalt", "Oak Bay", "View Royal", "Colwood", "Langford",
                "Metchosin", "Sooke", "Sidney", "North Saanich", "Central Saanich", "Nanaimo",
                "Ladysmith", "Parksville", "Qualicum Beach", "Port Alberni", "Ucluelet", "Tofino",
                "Courtenay", "Comox", "Cumberland", "Campbell River", "Gold River", "Tahsis", "Sayward",
                "Port McNeill", "Port Hardy", "Port Alice", "Zeballos", "Lake Cowichan", "Duncan",

                // Okanagan
                "Kelowna", "West Kelowna", "Vernon", "Penticton", "Summerland", "Peachland", "Lake Country",
                "Armstrong", "Enderby", "Lumby", "Coldstream", "Oliver", "Osoyoos", "Keremeos", "Princeton",
                "Sicamous", "Salmon Arm", "Revelstoke", "Kamloops"
            };

            var albertaCities = new List<string>
            {
                "Calgary", "Edmonton", "Red Deer", "Lethbridge", "St. Albert", "Medicine Hat", "Grande Prairie",
                "Airdrie", "Spruce Grove", "Leduc", "Fort Saskatchewan", "Lloydminster", "Camrose",
                "Chestermere", "Cochrane", "Okotoks", "High River", "Strathmore", "Canmore", "Banff"
            };

            allCities.UnionWith(bcCities);
            allCities.UnionWith(albertaCities);

            var groups = allCities.Select(city => new DestinationGroup { Name = city }).ToList();
            _db.DestinationGroups.AddRange(groups);
            await _db.SaveChangesAsync();
        }
    }
}