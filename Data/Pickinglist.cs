using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    [Index(nameof(SalesOrderNumber), IsUnique = true)]
    public class PickingList
    {
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string SalesOrderNumber { get; set; } = default!;

        [Required]
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        // ... other existing fields like OrderDate, ShipDate, CustomerName, ShipToAddress...
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ShipDate { get; set; }
        [MaxLength(128)]
        public string? CustomerName { get; set; }
        [MaxLength(256)]
        public string? ShipToAddress { get; set; }
        [MaxLength(128)]
        public string? SalesRep { get; set; }
        public int? TruckId { get; set; }
        public virtual Truck? Truck { get; set; }

        // --- ENHANCEMENTS ---
        public ShippingGroup ShippingGroup { get; set; } // Replaces string 'ShippingMethod'
        [MaxLength(64)]
        public string? DestinationRegion { get; set; } // For filtering/grouping
        [Precision(18, 3)]
        public decimal TotalWeight { get; set; } // Calculated from Items, stored for performance
        [Precision(18, 3)]
        public decimal RemainingWeight { get; set; } // To track partial shipments

        public PickingListStatus Status { get; set; }
        [MaxLength(512)]
        public string? Notes { get; set; }
        public virtual ICollection<PickingListItem> Items { get; set; } = new List<PickingListItem>();
    }

    public class PickingListItem : IEquatable<PickingListItem>
    {
        public int Id { get; set; }

        public int PickingListId { get; set; }
        public virtual PickingList? PickingList { get; set; }

        public int LineNumber { get; set; }

        [Required, MaxLength(64)]
        public string ItemId { get; set; } = default!;

        [Required, MaxLength(256)]
        public string ItemDescription { get; set; } = default!;

        [Precision(18, 3)]
        public decimal Quantity { get; set; }

        [MaxLength(16)]
        public string Unit { get; set; } = "EA";

        [Precision(18, 3)]
        public decimal? Width { get; set; }

        [Precision(18, 3)]
        public decimal? Length { get; set; }

        [Precision(18, 3)]
        public decimal? Weight { get; set; }

        [Precision(18, 3)]
        public decimal? PulledQuantity { get; set; }

        [Precision(18, 3)]
        public decimal? PulledWeight { get; set; }

        public PickingLineStatus Status { get; set; } = PickingLineStatus.Pending;
        public DateTime? ScheduledShipDate { get; set; }

        [NotMapped]
        public DateTime? EffectiveShipDate => ScheduledShipDate ?? PickingList?.ShipDate;

        public int? MachineId { get; set; }
        public virtual Machine? Machine { get; set; }

        public bool Equals(PickingListItem? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PickingListItem);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}