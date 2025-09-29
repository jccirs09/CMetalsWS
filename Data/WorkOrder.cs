using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Data
{
    public enum WorkOrderStatus
    {
        Draft,
        Pending,
        InProgress,
        Completed,
        Canceled,
        Awaiting
    }

    public enum WorkOrderPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    public class WorkOrder
    {
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string WorkOrderNumber { get; set; } = default!;

        [MaxLength(64)]
        public string? PdfWorkOrderNumber { get; set; }

        [MaxLength(64)]
        public string TagNumber { get; set; } = default!;

        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        public int? MachineId { get; set; }
        public Machine? Machine { get; set; }

        public MachineCategory MachineCategory { get; set; }

        public DateTime? DueDate { get; set; }

        public string? ParentItemId { get; set; }
        public string? ParentItemDescription { get; set; }
        [Precision(18, 2)]
        public decimal? ParentItemWeight { get; set; }
        public string? ParentItemLocation { get; set; }
        public string? Instructions { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ScheduledStartDate { get; set; }
        public DateTime? ScheduledEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
        public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Normal;

        [MaxLength(32)]
        public string? Shift { get; set; }

        [MaxLength(128)]
        public string? Operator { get; set; }

        public ICollection<WorkOrderItem> Items { get; set; } = new List<WorkOrderItem>();
    }

    public enum WorkOrderItemStatus
    {
        Pending,
        InProgress,
        Completed,
        Error
    }

    public class WorkOrderItem
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        public int? PickingListItemId { get; set; }
        public PickingListItem? PickingListItem { get; set; }

        [MaxLength(64)]
        public string ItemCode { get; set; } = default!;

        [MaxLength(256)]
        public string Description { get; set; } = default!;

        [MaxLength(64)]
        public string? SalesOrderNumber { get; set; }

        [MaxLength(128)]
        public string? CustomerName { get; set; }

        public decimal? OrderQuantity { get; set; }
        public decimal? OrderWeight { get; set; }

        public decimal? Width { get; set; }
        public decimal? Length { get; set; }

        public decimal? ProducedQuantity { get; set; }
        public decimal? ProducedWeight { get; set; }

        [MaxLength(64)]
        public string? Unit { get; set; }
        [MaxLength(64)]
        public string? Location { get; set; }

        public bool IsStockItem { get; set; }

        public WorkOrderItemStatus Status { get; set; } = WorkOrderItemStatus.Pending;

        [MaxLength(64)]
        public string? OriginalOrderLineItemId { get; set; }
    }
}