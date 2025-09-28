using System;
using System.ComponentModel.DataAnnotations;

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

        [MaxLength(512)]
        public string? FullAddress { get; set; }

        [MaxLength(100)]
        public string? BusinessHours { get; set; }

        [MaxLength(50)]
        public string? ContactNumber { get; set; }

        public bool Active { get; set; } = true;


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
        public int? DestinationRegionId { get; set; }
        public DestinationRegion? DestinationRegion { get; set; }

        public int? DestinationGroupId { get; set; }
        public DestinationGroup? DestinationGroup { get; set; }


        // --- Operational Details ---
        public int? MaxSkidCapacity { get; set; }
        public int? MaxSlitCoilWeight { get; set; }
        public TimeSpan? ReceivingHourStart { get; set; }
        public TimeSpan? ReceivingHourEnd { get; set; }

        public int ServiceTimeMinutes { get; set; } = 15; // Default service time

        [MaxLength(100)]
        public string? ContactName { get; set; }

        [MaxLength(100)]
        public string? ContactEmail { get; set; }

        [MaxLength(500)]
        public string? DeliveryNotes { get; set; }

        [MaxLength(500)]
        public string? AccessRestrictions { get; set; }

        public bool AppointmentRequired { get; set; }
        public PreferredTruckType PreferredTruckType { get; set; } = PreferredTruckType.FLATBED;

        public Priority Priority { get; set; } = Priority.NORMAL;

        // --- Timestamps ---
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedUtc { get; set; }
    }
}
