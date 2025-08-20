using CMetalsWS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            // Make sure the database exists. EnsureCreatedAsync is idempotent.
            await _context.Database.EnsureCreatedAsync();

            string[] roles = { "Admin", "Planner", "Supervisor", "Manager", "Operator", "Driver" };
            foreach (var roleName in roles)
            {
                // Look up role by name and create if not found
                var existingRole = await _roleManager.FindByNameAsync(roleName);
                if (existingRole == null)
                {
                    var role = new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() };
                    var result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        _logger.LogError("Failed to create role {Role}. Errors: {Errors}", roleName,
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            // Create admin user if it doesn’t already exist
            var adminUser = await _userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@scheduler.com",
                    EmailConfirmed = true
                };
                var createUserResult = await _userManager.CreateAsync(adminUser, "Admin123!");
                if (!createUserResult.Succeeded)
                {
                    _logger.LogError("Failed to create admin user. Errors: {Errors}",
                        string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                }
                else
                {
                    // Assign the Admin role
                    var addRoleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!addRoleResult.Succeeded)
                    {
                        _logger.LogError("Failed to add admin user to Admin role. Errors: {Errors}",
                            string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
    }
}
