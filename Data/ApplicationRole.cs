using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CMetalsWS.Data
{
    /// <summary>
    /// Custom role class.  Allows you to add descriptive information about each role.
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        [NotMapped]
        public List<string> Permissions { get; set; } = new();
    }
}
