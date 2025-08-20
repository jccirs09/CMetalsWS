using CMetalsWS.Data;
using Microsoft.AspNetCore.Identity;

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
            => await _roleManager.FindByIdAsync(id);

        public async Task CreateRoleAsync(ApplicationRole role)
        {
            await _roleManager.CreateAsync(role);
        }

        public async Task UpdateRoleAsync(ApplicationRole role)
        {
            var existing = await _roleManager.FindByIdAsync(role.Id);
            if (existing != null)
            {
                existing.Name = role.Name;
                existing.Description = role.Description;
                await _roleManager.UpdateAsync(existing);
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
