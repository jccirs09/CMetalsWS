using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public Branch? Branch { get; set; }

        // NEW: link to Customer (optional)
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? ShipDate { get; set; }

        // keep denormalized display fields
        [MaxLength(128)]
        public string? CustomerName { get; set; }

        [MaxLength(256)]
        public string? ShipToAddress { get; set; }

        [MaxLength(64)]
        public string? ShippingMethod { get; set; }

        public PickingListStatus Status { get; set; } = PickingListStatus.Pending;

        public int? TruckId { get; set; }
        public Truck? Truck { get; set; }

        public ICollection<PickingListItem> Items { get; set; } = new List<PickingListItem>();
    }

    public class PickingListItem
    {
        public int Id { get; set; }

        public int PickingListId { get; set; }
        public PickingList? PickingList { get; set; }

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

        public PickingLineStatus Status { get; set; } = PickingLineStatus.Pending;

        public int? MachineId { get; set; }
        public Machine? Machine { get; set; }
    }
}
