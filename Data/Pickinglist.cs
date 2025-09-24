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

        // Audit fields
        public DateTime? ScannedDate { get; set; }
        public string? ScannedById { get; set; }
        public virtual ApplicationUser? ScannedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedById { get; set; }
        public virtual ApplicationUser? ModifiedBy { get; set; }
        public string? AssignedToId { get; set; }
        public virtual ApplicationUser? AssignedTo { get; set; }
        [MaxLength(256)]
        public string? Destination { get; set; }

        public virtual ICollection<PickingListItem> Items { get; set; } = new List<PickingListItem>();
    }

    public class PickingListItem : IEquatable<PickingListItem>
    {
        public int Id { get; set; }

        public int PickingListId { get; set; }
        public virtual PickingList? PickingList { get; set; }

        public int LineNumber { get; set; }

        public int? InventoryItemId { get; set; }
        public virtual InventoryItem? InventoryItem { get; set; }

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

        public DateTime? ScheduledProcessingDate { get; set; }
        public DateTime? ScheduledShipDate { get; set; }

        [NotMapped]
        public DateTime? EffectiveShipDate => ScheduledShipDate;

        public int? MachineId { get; set; }
        public virtual Machine? Machine { get; set; }
        [MaxLength(128)]
        public string? Location { get; set; }
        [MaxLength(128)]
        public string? CoilId { get; set; }
        public bool Picked { get; set; }
        public string? PickedById { get; set; }
        public virtual ApplicationUser? PickedBy { get; set; }
        public DateTime? PickedAt { get; set; }
        public bool Packed { get; set; }
        public string? PackedById { get; set; }
        public virtual ApplicationUser? PackedBy { get; set; }
        public DateTime? PackedAt { get; set; }
        [MaxLength(128)]
        public string? PackingMaterial { get; set; }
        [MaxLength(512)]
        public string? PackingNotes { get; set; }
        public bool QualityChecked { get; set; }
        public string? QualityCheckedById { get; set; }
        public virtual ApplicationUser? QualityCheckedBy { get; set; }
        public DateTime? QualityCheckedAt { get; set; }
        [Precision(18, 3)]
        public decimal? ActualWeight { get; set; }
        [MaxLength(512)]
        public string? DamageNotes { get; set; }
        public bool Equals(PickingListItem? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            // For new entities, Id is 0. Use LineNumber for equality as it's unique within the list.
            if (Id == 0 && other.Id == 0)
            {
                return PickingListId == other.PickingListId && LineNumber == other.LineNumber;
            }

            return Id == other.Id;
        }

        public override bool Equals(object? obj) => Equals(obj as PickingListItem);

        public override int GetHashCode()
        {
            // For new entities, Id is 0. Use LineNumber for hash code.
            if (Id == 0)
            {
                return HashCode.Combine(PickingListId, LineNumber);
            }

            return Id.GetHashCode();
        }
    }
}
