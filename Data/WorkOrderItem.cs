using System;
using System.Collections.Generic;

namespace CMetalsWS.Data
{
    /// <summary>Represents a production or manufacturing order.</summary>
    public class WorkOrderOld
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; } = default!;
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }
        public int? MachineId { get; set; }
        public Machine? Machine { get; set; }
        public DateTime ScheduledStartDate { get; set; }
        public DateTime? ScheduledEndDate { get; set; }
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;

        public ICollection<WorkOrderItem> Items { get; set; } = new List<WorkOrderItem>();
    }

    /// <summary>An item or step within a work order.</summary>
    public class WorkOrderItemOld
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }
        public string ItemCode { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = default!;
    }
}
