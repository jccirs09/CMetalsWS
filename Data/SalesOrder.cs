using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    public enum SalesOrderStatus
    {
        New,
        PickingListPrinted,
        Scheduled,
        Completed,
        Canceled
    }

    public class SalesOrder
    {
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string OrderNumber { get; set; } = default!;

        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.New;

        public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    }

    public class SalesOrderItem
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        [Required, MaxLength(64)]
        public string ItemId { get; set; } = default!;

        // “Sheet” or “Coil”
        [Required, MaxLength(32)]
        public string ItemType { get; set; } = default!;

        // quantity for sheet (pieces), weight for coil
        public decimal Quantity { get; set; }

        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public decimal? Weight { get; set; }
        public bool InStock { get; set; } = false;
    }
}
