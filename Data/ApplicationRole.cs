using Microsoft.AspNetCore.Identity;

namespace CMetalsWS.Data
{
    /// <summary>
    /// Custom role class.  Allows you to add descriptive information about each role.
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
