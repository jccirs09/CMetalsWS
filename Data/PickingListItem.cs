using System;
using System.Collections.Generic;

namespace CMetalsWS.Data
{
    /// <summary>Represents a picking list or sales order to fulfill.</summary>
    public class PickingList
    {
        public int Id { get; set; }
        public string PickingListNumber { get; set; } = default!;
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string? CustomerName { get; set; }
        public PickingListStatus Status { get; set; } = PickingListStatus.Pending;

        // Optional truck assignment when shipped
        public int? TruckId { get; set; }
        public Truck? Truck { get; set; }

        public ICollection<PickingListItem> Items { get; set; } = new List<PickingListItem>();
    }

    /// <summary>Line item in a picking list.</summary>
    public class PickingListItem
    {
        public int Id { get; set; }
        public int PickingListId { get; set; }
        public PickingList? PickingList { get; set; }
        public string ItemCode { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = default!;
    }
}
