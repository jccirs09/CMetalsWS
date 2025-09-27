using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    [Index(nameof(Identifier), IsUnique = true)]
    public class Truck
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;

        [MaxLength(256)]
        public string? Description { get; set; }

        [Required, MaxLength(64)]
        public string Identifier { get; set; } = default!; // license plate or fleet number

        [Precision(18, 2)]
        public decimal CapacityWeight { get; set; }

        [Precision(18, 2)]
        public decimal CapacityVolume { get; set; }

        public bool IsActive { get; set; } = true;

        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        public string? DriverId { get; set; } // FK to AspNetUsers
        public ApplicationUser? Driver { get; set; }

        public int? DestinationRegionId { get; set; }
        public DestinationRegion? DestinationRegion { get; set; }

        public bool IsContractor { get; set; }
    }
}
