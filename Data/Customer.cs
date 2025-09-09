using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Data
{
    [Index(nameof(CustomerCode), IsUnique = true)]
    public class Customer
    {
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string CustomerCode { get; set; } = default!;

        [Required, MaxLength(256)]
        public string CustomerName { get; set; } = default!;

        // Display/original
        [MaxLength(512)]
        public string? Address { get; set; }

        // Normalized parts
        [MaxLength(128)] public string? Street1 { get; set; }
        [MaxLength(128)] public string? Street2 { get; set; }
        [MaxLength(96)]  public string? City { get; set; }
        [MaxLength(64)]  public string? Province { get; set; }  // e.g., "BC"
        [MaxLength(16)]  public string? PostalCode { get; set; }
        [MaxLength(64)]  public string? Country { get; set; }   // e.g., "Canada"

        // Geo
        [Column(TypeName = "decimal(9,6)")] public decimal? Latitude { get; set; }
        [Column(TypeName = "decimal(9,6)")] public decimal? Longitude { get; set; }
        [MaxLength(128)] public string? PlaceId { get; set; }

        // Routing categorizations
        public DestinationRegionCategory DestinationRegionCategory { get; set; }
        [MaxLength(64)] public string? DestinationGroupCategory { get; set; } // e.g., "SOUTH SURREY" or "NANAIMO"

        // Operations
        [MaxLength(128)] public string? ContactName { get; set; }
        [MaxLength(128)] public string? ContactEmail { get; set; }
        [MaxLength(48)]  public string? ContactNumber { get; set; }
        [MaxLength(128)] public string? Timezone { get; set; } = "America/Vancouver";
        public TimeOnly? TimeWindowStart { get; set; }  // default delivery window start
        public TimeOnly? TimeWindowEnd { get; set; }    // default delivery window end
        public int? ServiceTimeMinutes { get; set; }

        public DockType DockType { get; set; } = DockType.NONE;
        public PreferredTruckType PreferredTruckType { get; set; } = PreferredTruckType.VAN_5TON;
        public bool LiftgateRequired { get; set; }
        public bool AppointmentRequired { get; set; }
        public bool FerryRequired { get; set; }
        public bool TollRoutesAllowed { get; set; } = true;
        public PriorityLevel Priority { get; set; } = PriorityLevel.NORMAL;

        public bool Active { get; set; } = true;

        // Freeform
        public string? BusinessHours { get; set; }  // human-readable
        public string? DeliveryNotes { get; set; }
        public string? AccessRestrictions { get; set; }
        public string? CustomTags { get; set; } // CSV or JSON

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedUtc { get; set; }
    }
}
