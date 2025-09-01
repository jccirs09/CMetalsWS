using System.Security.Claims;
using CMetalsWS.Data;
using CMetalsWS.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

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

            // Create roles
            string[] roles = { "Admin", "Planner", "Supervisor", "Manager", "Operator", "Driver" };
            foreach (var r in roles)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new ApplicationRole { Name = r });
            }

            // Permission maps
            var all = Permissions.All().ToList();

            var manager = new[]
            {
                Permissions.Users.View,
                Permissions.Roles.View,
                Permissions.Branches.View, Permissions.Branches.Add, Permissions.Branches.Edit,
                Permissions.Machines.View, Permissions.Machines.Add, Permissions.Machines.Edit,
                Permissions.Trucks.View, Permissions.Trucks.Add, Permissions.Trucks.Edit,
                Permissions.WorkOrders.View, Permissions.WorkOrders.Add, Permissions.WorkOrders.Edit,
                Permissions.PickingLists.View, Permissions.PickingLists.Add, Permissions.PickingLists.Edit
            };

            var planner = new[]
            {
                Permissions.WorkOrders.View, Permissions.WorkOrders.Add, Permissions.WorkOrders.Edit,
                Permissions.PickingLists.View, Permissions.PickingLists.Add, Permissions.PickingLists.Edit
            };

            var supervisor = new[]
            {
                Permissions.WorkOrders.View, Permissions.WorkOrders.Edit,
                Permissions.PickingLists.View, Permissions.PickingLists.Edit
            };

            var oper = new[]
            {
                Permissions.WorkOrders.View,
                Permissions.PickingLists.View
            };

            var driver = new[]
            {
                Permissions.PickingLists.View
            };

            var map = new Dictionary<string, IEnumerable<string>>
            {
                ["Admin"] = all,
                ["Manager"] = manager,
                ["Planner"] = planner,
                ["Supervisor"] = supervisor,
                ["Operator"] = oper,
                ["Driver"] = driver
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
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var created = await _userManager.CreateAsync(admin, "Admin123!");
                if (!created.Succeeded)
                {
                    _logger.LogError("Admin create failed: {E}", string.Join(", ", created.Errors.Select(e => e.Description)));
                    return;
                }

                await _userManager.AddToRoleAsync(admin, "Admin");
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
    }
}
