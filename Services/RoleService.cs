using CMetalsWS.Data;
using CMetalsWS.Security;
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

        public List<ApplicationRole> GetRoles() => _roleManager.Roles.ToList();

        public async Task<ApplicationRole?> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                role.Permissions = claims
                    .Where(c => c.Type == Permissions.ClaimType)
                    .Select(c => c.Value)
                    .ToList();
            }
            return role;
        }

        public async Task CreateRoleAsync(ApplicationRole role)
        {
            var newRole = new ApplicationRole { Name = role.Name, Description = role.Description };
            await _roleManager.CreateAsync(newRole);
            foreach (var permission in role.Permissions)
            {
                await _roleManager.AddClaimAsync(newRole, new Claim(Permissions.ClaimType, permission));
            }
        }

        public async Task UpdateRoleAsync(ApplicationRole role)
        {
            var existing = await _roleManager.FindByIdAsync(role.Id);
            if (existing != null)
            {
                existing.Name = role.Name;
                existing.Description = role.Description;
                await _roleManager.UpdateAsync(existing);

                var claims = await _roleManager.GetClaimsAsync(existing);
                var permissionClaims = claims.Where(c => c.Type == Permissions.ClaimType).ToList();

                foreach (var claim in permissionClaims)
                {
                    await _roleManager.RemoveClaimAsync(existing, claim);
                }

                foreach (var permission in role.Permissions)
                {
                    await _roleManager.AddClaimAsync(existing, new Claim(Permissions.ClaimType, permission));
                }
            }
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
