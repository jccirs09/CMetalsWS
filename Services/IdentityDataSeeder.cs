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
    }
}
