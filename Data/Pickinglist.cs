using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    [Index(nameof(BranchId), nameof(SalesOrderNumber), IsUnique = true)]
    public class PickingList
    {
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public required string SalesOrderNumber { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? ShipDate { get; set; }

        // Business header fields
        [MaxLength(256)]
        public string? SoldTo { get; set; }

        [MaxLength(256)]
        public string? ShipTo { get; set; }

        [MaxLength(128)]
        public string? SalesRep { get; set; }

        [MaxLength(128)]
        public string? Buyer { get; set; }
        public DateTime? PrintDateTime { get; set; }

        // Branch / Customer
        [Required]
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; } = null!;

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
        public int? DestinationRegionId { get; set; }
        public virtual DestinationRegion? DestinationRegion { get; set; }

        // Totals
        [Precision(18, 3)]
        public decimal TotalWeight { get; set; }

        [Precision(18, 3)]
        public decimal RemainingWeight { get; set; }


        public PickingListStatus Status { get; set; }

        public int Priority { get; set; } = 99;

        public virtual ICollection<PickingListItem> Items { get; set; } = new List<PickingListItem>();
    }

    public class PickingListItem : IEquatable<PickingListItem>
    {
        public int Id { get; set; }

        public int PickingListId { get; set; }
        public virtual PickingList? PickingList { get; set; }

        public int LineNumber { get; set; }

        [Required, MaxLength(64)]
        public required string ItemId { get; set; }

        [Required, MaxLength(256)]
        public required string ItemDescription { get; set; }

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
        public decimal PulledWeight { get; set; }

        public PickingLineStatus Status { get; set; } = PickingLineStatus.Pending;
        public DateTime? ScheduledShipDate { get; set; }

        [NotMapped]
        public DateTime? EffectiveShipDate => ScheduledShipDate;

        public int? MachineId { get; set; }
        public virtual Machine? Machine { get; set; }

        public bool Equals(PickingListItem? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object? obj) => Equals(obj as PickingListItem);
        public override int GetHashCode() => Id.GetHashCode();
    }
}
