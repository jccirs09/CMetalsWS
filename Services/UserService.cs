using CMetalsWS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<ApplicationUser>> GetUsersAsync()
        {
            return await _userManager.Users.Include(u => u.Branch).AsNoTracking().ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await _userManager.Users.Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<string>> GetRolesForUserAsync(ApplicationUser user)
        {
            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        public List<ApplicationRole> GetAllRoles()
        {
            return _roleManager.Roles.ToList();
        }

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, IEnumerable<string> roles)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("TestClaim", "TestValue"));
                if (roles.Any())
                {
                    await _userManager.AddToRolesAsync(user, roles);
                }
            }
            return result;
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            var existing = await _userManager.FindByIdAsync(user.Id);
            if (existing == null) return IdentityResult.Failed();

            existing.UserName = user.UserName;
            existing.Email = user.Email;
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.BranchId = user.BranchId;

            var result = await _userManager.UpdateAsync(existing);
            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(existing);
                var toRemove = currentRoles.Except(roles).ToList();
                var toAdd = roles.Except(currentRoles).ToList();
                if (toRemove.Any())
                    await _userManager.RemoveFromRolesAsync(existing, toRemove);
                if (toAdd.Any())
                    await _userManager.AddToRolesAsync(existing, toAdd);
            }
            return result;
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }
    }
}
