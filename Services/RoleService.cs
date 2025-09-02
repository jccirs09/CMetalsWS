using CMetalsWS.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CMetalsWS.Services
{
    public class RoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleService(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<List<ApplicationRole>> GetRolesAsync()
        {
            var roles = _roleManager.Roles.ToList();
            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                role.Permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
            }
            return roles;
        }

        public async Task<ApplicationRole?> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                role.Permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
            }
            return role;
        }

        public async Task<IdentityResult> CreateRoleAsync(ApplicationRole role)
        {
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                foreach (var permission in role.Permissions)
                {
                    await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
            }
            return result;
        }

        public async Task<IdentityResult> UpdateRoleAsync(ApplicationRole role)
        {
            var existingRole = await _roleManager.FindByIdAsync(role.Id);
            if (existingRole == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
            }

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;

            var result = await _roleManager.UpdateAsync(existingRole);
            if (!result.Succeeded)
            {
                return result;
            }

            var existingClaims = await _roleManager.GetClaimsAsync(existingRole);
            var existingPermissions = existingClaims.Where(c => c.Type == "Permission").ToList();
            var newPermissions = role.Permissions;

            var permissionsToRemove = existingPermissions.Where(ep => !newPermissions.Contains(ep.Value)).ToList();
            foreach (var claim in permissionsToRemove)
            {
                await _roleManager.RemoveClaimAsync(existingRole, claim);
            }

            var permissionsToAdd = newPermissions.Where(np => !existingPermissions.Any(ep => ep.Value == np)).ToList();
            foreach (var permission in permissionsToAdd)
            {
                await _roleManager.AddClaimAsync(existingRole, new Claim("Permission", permission));
            }

            return IdentityResult.Success;
        }

        public async Task DeleteRoleAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }
        }
    }
}
