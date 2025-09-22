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
        public string FullName => $"{FirstName} {LastName}";
        public string? Avatar { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsOnline { get; set; } = true;

        // Foreign key to associate a user with a branch (nullable – a user may not belong to a specific branch)
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public int? ShiftId { get; set; }
        public Shift? Shift { get; set; }
        public int? MachineId { get; set; }
        public Machine? Machine { get; set; }
        // Navigation property for many‑to‑many roles via Identity tables comes automatically with Identity
    }
}
