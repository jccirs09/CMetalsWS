using System.Security.Claims;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CMetalsWS.Services
{
    public class AppClaimsPrincipalFactory
        : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // FIX: must be public to match the base member signature
        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity!;

            if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            }

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var rc in roleClaims)
                {
                    if (!identity.HasClaim(rc.Type, rc.Value))
                        identity.AddClaim(rc);
                }
            }

            return principal;
        }
    }
}
