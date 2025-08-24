using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    /// <summary>A production machine on a branch site.</summary>
    public class Machine
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Model { get; set; }
        public string? Description { get; set; }
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        [MaxLength(64)]
        public string? Category { get; set; }
    }
}
