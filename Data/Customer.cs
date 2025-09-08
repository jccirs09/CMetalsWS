using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class Customer
    {
        public int Id { get; set; }

        // --- Basic Information ---
        [Required, MaxLength(16)]
        public string CustomerCode { get; set; } = default!;

        [Required, MaxLength(200)]
        public string CustomerName { get; set; } = default!;

        [MaxLength(256)]
        public string? FullAddress { get; set; }        // auto-generated from address parts

        [MaxLength(100)]
        public string? BusinessHours { get; set; }

        [MaxLength(50)]
        public string? ContactNumber { get; set; }

        public bool Active { get; set; } = true;


        // --- Routing & Geocoding ---
        [MaxLength(256)]
        public string? PlaceId { get; set; } // Google Places API ID

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Longitude { get; set; }

        [MaxLength(128)]
        public string? Street1 { get; set; }

        [MaxLength(128)]
        public string? Street2 { get; set; }

        [MaxLength(64)]
        public string? City { get; set; }

        [MaxLength(64)]
        public string? Province { get; set; }

        [MaxLength(16)]
        public string? PostalCode { get; set; }

        [MaxLength(64)]
        public string? Country { get; set; }

        // --- Categorization ---
        public DestinationRegionCategory DestinationRegionCategory { get; set; }

        [MaxLength(64)]
        public string? DestinationGroupCategory { get; set; }


        // --- Operational Details ---
        public TimeSpan? TimeWindowStart { get; set; }
        public TimeSpan? TimeWindowEnd { get; set; }
        public int ServiceTimeMinutes { get; set; } = 30; // Default service time

        [MaxLength(100)]
        public string? ContactName { get; set; }

        [MaxLength(100)]
        public string? ContactEmail { get; set; }

        [MaxLength(500)]
        public string? DeliveryNotes { get; set; }

        [MaxLength(500)]
        public string? AccessRestrictions { get; set; }

        public DockType DockType { get; set; } = DockType.NONE;
        public bool LiftgateRequired { get; set; }
        public bool AppointmentRequired { get; set; }
        public PreferredTruckType PreferredTruckType { get; set; } = PreferredTruckType.VAN_5TON;
        public bool FerryRequired { get; set; }
        public bool TollRoutesAllowed { get; set; } = true;
        public Priority Priority { get; set; } = Priority.NORMAL;

        [MaxLength(64)]
        public string Timezone { get; set; } = "America/Vancouver";

        [MaxLength(256)]
        public string? CustomTags { get; set; } // e.g., "tag1,tag2,tag3"


        // --- Timestamps ---
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedUtc { get; set; }
    }
}
