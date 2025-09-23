using CMetalsWS.Data;
using CMetalsWS.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CMetalsWS.Services
{
    public class IdentityDataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IdentityDataSeeder> _logger;

        public IdentityDataSeeder(
            ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<IdentityDataSeeder> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            await SeedBranchesAsync();
            await SeedCityCentroidsAsync();

            // Create roles
            string[] roles = { "Admin", "Planner", "Supervisor", "Manager", "Operator", "Driver", "Viewer" };
            foreach (var r in roles)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new ApplicationRole { Name = r, Description = $"{r} role" });
            }

            // Permission maps
            var all = Permissions.All().ToList();

            var plannerPermissions = new[]
            {
                Permissions.PickingLists.View, Permissions.PickingLists.Add, Permissions.PickingLists.Assign, Permissions.PickingLists.ManageLoads,
                Permissions.WorkOrders.View, Permissions.WorkOrders.Add, Permissions.WorkOrders.Schedule,
                Permissions.Trucks.View,
                Permissions.Machines.View,
                Permissions.Branches.View,
                Permissions.Dashboards.View,
                Permissions.Customers.View, Permissions.Customers.Edit
            };

            var supervisorPermissions = plannerPermissions.Concat(new[]
            {
                Permissions.WorkOrders.Approve,
                Permissions.PickingLists.Dispatch,
                Permissions.Customers.Add
            }).Distinct().ToArray();

            var managerPermissions = supervisorPermissions.Concat(new[]
            {
                Permissions.Users.View,
                Permissions.Roles.View,
                Permissions.Branches.Edit,
                Permissions.Machines.Edit,
                Permissions.Trucks.Edit,
                Permissions.PickingLists.Delete,
                Permissions.WorkOrders.Delete,
                Permissions.Reports.View,
                Permissions.Reports.Export
            }).Distinct().ToArray();

            var operatorPermissions = new[]
            {
                Permissions.PickingLists.View, Permissions.PickingLists.Add, Permissions.PickingLists.Edit, Permissions.PickingLists.Assign,
                Permissions.WorkOrders.View, Permissions.WorkOrders.Add, Permissions.WorkOrders.Edit, Permissions.WorkOrders.Process, Permissions.WorkOrders.Schedule
            };

            var driverPermissions = new[]
            {
                Permissions.PickingLists.View, Permissions.PickingLists.Dispatch
            };

            var viewerPermissions = new[]
            {
                Permissions.Dashboards.View,
                Permissions.Reports.View,
                Permissions.Users.View,
                Permissions.Roles.View,
                Permissions.Branches.View,
                Permissions.Machines.View,
                Permissions.Trucks.View,
                Permissions.WorkOrders.View,
                Permissions.PickingLists.View
            };

            var map = new Dictionary<string, IEnumerable<string>>
            {
                ["Admin"] = all,
                ["Manager"] = managerPermissions,
                ["Supervisor"] = supervisorPermissions,
                ["Planner"] = plannerPermissions,
                ["Operator"] = operatorPermissions,
                ["Driver"] = driverPermissions,
                ["Viewer"] = viewerPermissions
            };

            // Apply permission claims to each role
            foreach (var kv in map)
            {
                var role = await _roleManager.FindByNameAsync(kv.Key);
                if (role is null) continue;

                var existing = await _roleManager.GetClaimsAsync(role);
                var existingPerms = existing
                    .Where(c => c.Type == Permissions.ClaimType)
                    .Select(c => c.Value)
                    .ToHashSet();

                foreach (var perm in kv.Value.Distinct())
                {
                    if (!existingPerms.Contains(perm))
                        await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, perm));
                }
            }

            // Seed admin user
            var adminEmail = "admin@example.com";
            var admin = await _userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                var newAdminUser = new ApplicationUser { UserName = "admin", Email = adminEmail, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(newAdminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
                else
                {
                     _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                     return;
                }
            }

            // Re-fetch the admin user to ensure we have a valid instance for subsequent operations.
            admin = await _userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                _logger.LogError("Admin user could not be found or created. Aborting further seeding.");
                return;
            }

            await SeedUserClaimsAsync();
            await SeedChatDataAsync(admin);
        }

        private async Task SeedChatDataAsync(ApplicationUser admin)
        {
            if (await _context.ChatMessages.AnyAsync()) return;

            async Task<ApplicationUser?> CreateChatUser(string name, string role)
            {
                if (await _userManager.FindByNameAsync(name) != null) return await _userManager.FindByNameAsync(name);

                var user = new ApplicationUser { UserName = name, Email = $"{name}@example.com", EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, "User123!");
                if (!result.Succeeded)
                {
                    _logger.LogError($"Failed to create user '{name}'.");
                    return null;
                }
                await _userManager.AddToRoleAsync(user, role);
                return await _userManager.FindByNameAsync(name);
            }

            var user1 = await CreateChatUser("user1", "Viewer");
            var user2 = await CreateChatUser("user2", "Viewer");

            if (admin == null || user1 == null || user2 == null)
            {
                _logger.LogError("A required user for chat seeding was not found or created.");
                return;
            }

            var group = await _context.ChatGroups.FirstOrDefaultAsync(g => g.Name == "General");
            if (group == null)
            {
                group = new ChatGroup { Name = "General" };
                await _context.ChatGroups.AddAsync(group);
                await _context.SaveChangesAsync();

                await _context.ChatGroupUsers.AddRangeAsync(
                    new ChatGroupUser { ChatGroupId = group.Id, UserId = admin.Id },
                    new ChatGroupUser { ChatGroupId = group.Id, UserId = user1.Id }
                );
            }

            await _context.ChatMessages.AddRangeAsync(
                new ChatMessage { ChatGroupId = group.Id, SenderId = admin.Id, Content = "Welcome to the general chat!", Timestamp = DateTime.UtcNow.AddMinutes(-10) },
                new ChatMessage { ChatGroupId = group.Id, SenderId = user1.Id, Content = "Glad to be here!", Timestamp = DateTime.UtcNow.AddMinutes(-5) },
                new ChatMessage { SenderId = admin.Id, RecipientId = user2.Id, Content = "Hi User 2, this is a private message.", Timestamp = DateTime.UtcNow.AddMinutes(-20) },
                new ChatMessage { SenderId = user2.Id, RecipientId = admin.Id, Content = "Hi Admin, I got it!", Timestamp = DateTime.UtcNow.AddMinutes(-15) }
            );

            await _context.SaveChangesAsync();
            _logger.LogInformation("Chat data seeded successfully.");
        }

        private async Task SeedUserClaimsAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
                {
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.NameIdentifier, user.Id));
                }
            }
        }

        private async Task SeedBranchesAsync()
        {
            if (await _context.Branches.AnyAsync())
            {
                return; // Branches have already been seeded
            }

            var branches = new List<Branch>
            {
                new Branch { Code = "DL", Name = "DELTA", AddressLine = "7630 Berg Road", City = "Delta", Province = "BC", PostalCode = "V4G 1G4" },
                new Branch { Code = "SU", Name = "SURREY", AddressLine = "#104 - 19433 96th Avenue", City = "Surrey", Province = "BC", PostalCode = "V4N 4C4" },
                new Branch { Code = "CG", Name = "CALGARY", AddressLine = "5535 53rd Ave SE", City = "Calgary", Province = "AB", PostalCode = "T2C 4N2" },
                new Branch { Code = "ED", Name = "EDMONTON", AddressLine = "9525 60 Avenue NW", City = "Edmonton", Province = "AB", PostalCode = "T6E 0C3" },
                new Branch { Code = "SA", Name = "SASKATOON", AddressLine = "3062 Millar Ave", City = "Saskatoon", Province = "SK", PostalCode = "S7K 5X9" },
                new Branch { Code = "BR", Name = "BRANDON", AddressLine = "33rd St East & Hwy 10", City = "Brandon", Province = "MB", PostalCode = "R7A 5Y4" },
                new Branch { Code = "WP", Name = "WINNIPEG", AddressLine = "1540 Seel Ave", City = "Winnipeg", Province = "MB", PostalCode = "R3T 4Z6" },
                new Branch { Code = "MI", Name = "MISSISSAUGA", AddressLine = "75 Skyway Dr, Unit-B", City = "Mississauga", Province = "ON", PostalCode = "L5W 0H2" },
                new Branch { Code = "HA", Name = "HAMILTON", AddressLine = "1632 Burlington St E", City = "Hamilton", Province = "ON", PostalCode = "L8H 3L3" },
                new Branch { Code = "DO", Name = "DORVAL", AddressLine = "1535 Hymus Blvd", City = "Dorval", Province = "QC", PostalCode = "H9P 1J5" }
            };

            var startTime = new TimeOnly(5, 0); // 5 AM
            var endTime = new TimeOnly(23, 59); // Midnight

            foreach (var branch in branches)
            {
                branch.StartTime = startTime;
                branch.EndTime = endTime;
            }

            _context.Branches.AddRange(branches);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCityCentroidsAsync()
        {
            if (await _context.CityCentroids.AnyAsync())
            {
                return; // Data has already been seeded
            }

            var centroids = new List<CityCentroid>
            {
                // Metro Vancouver (LOCAL)
                new CityCentroid { City = "Vancouver", Province = "BC", Latitude = 49.2827m, Longitude = -123.1207m },
                new CityCentroid { City = "Burnaby", Province = "BC", Latitude = 49.2465m, Longitude = -122.9945m },
                new CityCentroid { City = "Surrey", Province = "BC", Latitude = 49.1913m, Longitude = -122.8490m },
                new CityCentroid { City = "Richmond", Province = "BC", Latitude = 49.1666m, Longitude = -123.1336m },
                new CityCentroid { City = "Coquitlam", Province = "BC", Latitude = 49.2838m, Longitude = -122.7932m },
                new CityCentroid { City = "Langley", Province = "BC", Latitude = 49.1042m, Longitude = -122.6604m },
                new CityCentroid { City = "Delta", Province = "BC", Latitude = 49.0846m, Longitude = -123.0586m },
                new CityCentroid { City = "North Vancouver", Province = "BC", Latitude = 49.3200m, Longitude = -123.0700m },
                new CityCentroid { City = "West Vancouver", Province = "BC", Latitude = 49.3200m, Longitude = -123.1400m },
                new CityCentroid { City = "New Westminster", Province = "BC", Latitude = 49.2057m, Longitude = -122.9110m },

                // Vancouver Island (ISLAND)
                new CityCentroid { City = "Victoria", Province = "BC", Latitude = 48.4284m, Longitude = -123.3656m },
                new CityCentroid { City = "Nanaimo", Province = "BC", Latitude = 49.1659m, Longitude = -123.9401m },
                new CityCentroid { City = "Courtenay", Province = "BC", Latitude = 49.6869m, Longitude = -124.9946m },
                new CityCentroid { City = "Comox", Province = "BC", Latitude = 49.6729m, Longitude = -124.9277m },
                new CityCentroid { City = "Campbell River", Province = "BC", Latitude = 50.0248m, Longitude = -125.2446m },

                // Okanagan (OKANAGAN)
                new CityCentroid { City = "Kelowna", Province = "BC", Latitude = 49.8880m, Longitude = -119.4960m },
                new CityCentroid { City = "Penticton", Province = "BC", Latitude = 49.4906m, Longitude = -119.5895m },
                new CityCentroid { City = "Vernon", Province = "BC", Latitude = 50.2671m, Longitude = -119.2721m },
                new CityCentroid { City = "Summerland", Province = "BC", Latitude = 49.6052m, Longitude = -119.6685m },
                new CityCentroid { City = "Peachland", Province = "BC", Latitude = 49.7716m, Longitude = -119.7369m },

                // Other (OUT_OF_TOWN)
                new CityCentroid { City = "Prince George", Province = "BC", Latitude = 53.9171m, Longitude = -122.7497m },
                new CityCentroid { City = "Kamloops", Province = "BC", Latitude = 50.6745m, Longitude = -120.3273m },
                new CityCentroid { City = "Fort St. John", Province = "BC", Latitude = 56.2499m, Longitude = -120.8492m },
                new CityCentroid { City = "Whistler", Province = "BC", Latitude = 50.1163m, Longitude = -122.9574m },
                new CityCentroid { City = "Squamish", Province = "BC", Latitude = 49.7018m, Longitude = -123.1555m },
                new CityCentroid { City = "Chilliwack", Province = "BC", Latitude = 49.1579m, Longitude = -121.9515m },
                new CityCentroid { City = "Abbotsford", Province = "BC", Latitude = 49.0505m, Longitude = -122.3045m },
            };

            _context.CityCentroids.AddRange(centroids);
            await _context.SaveChangesAsync();
        }
    }
}
