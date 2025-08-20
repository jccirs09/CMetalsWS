using Microsoft.AspNetCore.Identity;
using static MudBlazor.Icons.Custom;

namespace CMetalsWS.Data
{
    /// <summary>
    /// Custom application user.  Extends IdentityUser with profile and warehouse‑specific fields.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Optional personal information
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Foreign key to associate a user with a branch (nullable – a user may not belong to a specific branch)
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }

        // Navigation property for many‑to‑many roles via Identity tables comes automatically with Identity
    }
}
