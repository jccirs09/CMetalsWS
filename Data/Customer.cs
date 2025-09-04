using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, MaxLength(16)]
        public string CustomerCode { get; set; } = default!;

        [Required, MaxLength(200)]
        public string CustomerName { get; set; } = default!;

        [MaxLength(32)]
        public string? LocationCode { get; set; }   // used for load grouping/routing

        [MaxLength(256)]
        public string? Address { get; set; }        // single-line address (for now)

        public bool IsActive { get; set; } = true;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedUtc { get; set; }

        // NEW: Fields to store default shipping info to speed up order entry.
        public ShippingGroup? DefaultShippingGroup { get; set; }
        public string? DefaultDestinationRegion { get; set; }
    }
}
