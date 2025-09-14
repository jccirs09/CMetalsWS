using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Data
{
    public enum WorkOrderStatus
    {
        Draft = 0,
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Canceled = 4,
        Awaiting = 5,
        Paused = 6
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

        public DateTime DueDate { get; set; }

        public string? ParentItemId { get; set; }
        public string? Instructions { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ScheduledStartDate { get; set; }
        public DateTime ScheduledEndDate { get; set; }

        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;

        [MaxLength(32)]
        public string? Shift { get; set; }

        public ICollection<WorkOrderItem> Items { get; set; } = new List<WorkOrderItem>();

        public int EstimatedMinutes { get; set; } = 0;

        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }

        [Timestamp] public byte[]? RowVersion { get; set; }

        public int? CoilInventoryId { get; set; }
        [MaxLength(64)] public string? CoilItemId { get; set; }
        [MaxLength(256)] public string? CoilDescription { get; set; }
        [Precision(18,3)] public decimal? CoilWeightAtWOStartLbs { get; set; }
        [MaxLength(64)] public string? CoilLocationAtWOStart { get; set; }
        [MaxLength(64)] public string? CoilMillRef { get; set; }
        public DateTime? CoilSnapshotAt { get; set; }

        public int? ActiveCoilUsageId { get; set; }
        public WorkOrderCoilUsage? ActiveCoilUsage { get; set; }
        public ICollection<WorkOrderCoilUsage> CoilUsages { get; set; } = new List<WorkOrderCoilUsage>();

        [NotMapped]
        public double Progress0to1 =>
            (Status == WorkOrderStatus.InProgress && EstimatedMinutes > 0 && ActualStart.HasValue)
                ? Math.Clamp((DateTime.UtcNow - ActualStart.Value).TotalMinutes / EstimatedMinutes, 0, 1)
                : 0d;

        [NotMapped] public DateTime? Eta => ScheduledEndDate;
        [NotMapped] public int SwapCount => Math.Max(0, CoilUsages.Count - 1);
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
    }

    public enum CoilSwapReason : byte
    {
        Initial = 0,
        SwapDefective = 1,
        SwapEmpty = 2,
        SwapOther = 3
    }

    public class WorkOrderCoilUsage
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; } = null!;

        public int Sequence { get; set; }

        public int CoilInventoryId { get; set; }
        [MaxLength(64)] public string CoilTagNumber { get; set; } = "";
        [MaxLength(64)] public string? CoilItemId { get; set; }
        [MaxLength(256)] public string? CoilDescription { get; set; }

        [Precision(18,3)] public decimal? StartWeightLbs { get; set; }
        [Precision(18,3)] public decimal? EndWeightLbs { get; set; }
        [MaxLength(64)] public string? FromLocation { get; set; }
        [MaxLength(64)] public string? ToLocation { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public CoilSwapReason Reason { get; set; } = CoilSwapReason.Initial;
        [MaxLength(256)] public string? Notes { get; set; }

        [NotMapped]
        public decimal? ConsumedWeightLbs =>
            (StartWeightLbs.HasValue && EndWeightLbs.HasValue) ? StartWeightLbs - EndWeightLbs : null;
    }
}