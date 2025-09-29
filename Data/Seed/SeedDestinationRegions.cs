using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CMetalsWS.Data.Seed
{
    public static class SeedDestinationRegions
    {
        public static async Task RunAsync(ApplicationDbContext db, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            var plannerRole = await roleManager.FindByNameAsync("Planner");
            if (plannerRole == null)
            {
                // Cannot seed without a Planner role
                return;
            }

            var planners = await userManager.GetUsersInRoleAsync("Planner");
            if (!planners.Any())
            {
                // Cannot seed without at least one planner
                return;
            }

            var regions = new List<DestinationRegion>
            {
                new() { Name = "Local Delivery", Type = "local", Description = "Same-day and next-day deliveries within metro area", RequiresPooling = false },
                new() { Name = "Multi Out of Town Lanes", Type = "out-of-town", Description = "Regional deliveries to multiple towns and cities", RequiresPooling = true },
                new() { Name = "Island Pool Trucks", Type = "island-pool", Description = "Consolidated ferry-dependent deliveries to Vancouver Island", RequiresPooling = true },
                new() { Name = "Okanagan Pool Trucks", Type = "okanagan-pool", Description = "Pooled deliveries to Okanagan Valley region", RequiresPooling = true },
                new() { Name = "Customer Pickup", Type = "customer-pickup", Description = "Customer self-pickup coordination and scheduling", RequiresPooling = false }
            };

            var existingRegions = await db.DestinationRegions.ToListAsync();
            var rng = new System.Random();

            foreach (var region in regions)
            {
                var existingRegion = existingRegions.FirstOrDefault(r => r.Name == region.Name);
                if (existingRegion == null)
                {
                    region.CoordinatorId = planners[rng.Next(planners.Count)].Id;
                    db.DestinationRegions.Add(region);
                }
                else
                {
                    existingRegion.Type = region.Type;
                    existingRegion.Description = region.Description;
                    existingRegion.RequiresPooling = region.RequiresPooling;
                    if (string.IsNullOrEmpty(existingRegion.CoordinatorId))
                    {
                        existingRegion.CoordinatorId = planners[rng.Next(planners.Count)].Id;
                    }
                }
            }

            await db.SaveChangesAsync();
        }
    }
}