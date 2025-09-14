using System;
using System.Linq;
using CMetalsWS.Data;

namespace CMetalsWS.Domain
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }

    public static class WorkOrderRules
    {
        public static void ApplyCoilSnapshot(WorkOrder workOrder, InventoryItem coil, IClock clock)
        {
            workOrder.CoilItemId = coil.ItemId;
            workOrder.CoilDescription = coil.Description;
            workOrder.CoilWeightAtWOStartLbs = coil.Snapshot;
            workOrder.CoilLocationAtWOStart = coil.Location;
            workOrder.CoilSnapshotAt = clock.UtcNow;
            // workOrder.CoilMillRef is not in InventoryItem, leave as-is.
        }

        public static void ValidateCanStart(WorkOrder workOrder)
        {
            if (workOrder.Status != WorkOrderStatus.Pending && workOrder.Status != WorkOrderStatus.Paused)
                throw new DomainException($"Cannot start a work order with status '{workOrder.Status}'.");
        }

        public static void ApplyStart(WorkOrder workOrder, ApplicationUser user, InventoryItem? initialCoil, IClock clock)
        {
            var wasAlreadyStarted = workOrder.ActualStart.HasValue;

            workOrder.Status = WorkOrderStatus.InProgress;
            if (!wasAlreadyStarted)
            {
                workOrder.ActualStart = clock.UtcNow;
            }
            workOrder.LastUpdatedBy = user.UserName;
            workOrder.LastUpdatedDate = clock.UtcNow;

            if (!wasAlreadyStarted && workOrder.CoilInventoryId.HasValue)
            {
                if (initialCoil == null)
                    throw new DomainException("Initial coil inventory item not found.");

                var usage = new WorkOrderCoilUsage
                {
                    WorkOrderId = workOrder.Id,
                    Sequence = 1,
                    CoilInventoryId = initialCoil.Id,
                    CoilTagNumber = initialCoil.TagNumber ?? "N/A",
                    CoilItemId = initialCoil.ItemId,
                    CoilDescription = initialCoil.Description,
                    StartWeightLbs = initialCoil.Snapshot,
                    FromLocation = initialCoil.Location,
                    StartedAt = clock.UtcNow,
                    Reason = CoilSwapReason.Initial,
                };
                workOrder.CoilUsages.Add(usage);
                workOrder.ActiveCoilUsage = usage;
            }
        }

        public static void ValidateCanPause(WorkOrder workOrder)
        {
            if (workOrder.Status != WorkOrderStatus.InProgress)
                throw new DomainException($"Cannot pause a work order with status '{workOrder.Status}'.");
        }

        public static void ApplyPause(WorkOrder workOrder, ApplicationUser user, IClock clock)
        {
            workOrder.Status = WorkOrderStatus.Paused;
            workOrder.LastUpdatedBy = user.UserName;
            workOrder.LastUpdatedDate = clock.UtcNow;
        }

        public static void ValidateCanResume(WorkOrder workOrder)
        {
            if (workOrder.Status != WorkOrderStatus.Paused)
                throw new DomainException($"Cannot resume a work order with status '{workOrder.Status}'.");
        }

        public static void ApplyResume(WorkOrder workOrder, ApplicationUser user, IClock clock)
        {
            workOrder.Status = WorkOrderStatus.InProgress;
            workOrder.LastUpdatedBy = user.UserName;
            workOrder.LastUpdatedDate = clock.UtcNow;
        }

        public static void ValidateCanComplete(WorkOrder workOrder)
        {
            if (workOrder.Status != WorkOrderStatus.InProgress && workOrder.Status != WorkOrderStatus.Paused)
                throw new DomainException($"Cannot complete a work order with status '{workOrder.Status}'.");
        }

        public static void ApplyComplete(WorkOrder workOrder, ApplicationUser user, IClock clock)
        {
            workOrder.Status = WorkOrderStatus.Completed;
            workOrder.ActualEnd = clock.UtcNow;
            workOrder.LastUpdatedBy = user.UserName;
            workOrder.LastUpdatedDate = clock.UtcNow;

            if (workOrder.ActiveCoilUsage is not null)
            {
                workOrder.ActiveCoilUsage.EndedAt = clock.UtcNow;
            }
            workOrder.ActiveCoilUsageId = null;
        }

        public static void ValidateCanSwapCoil(WorkOrder workOrder)
        {
            if (workOrder.Status != WorkOrderStatus.InProgress && workOrder.Status != WorkOrderStatus.Paused)
                throw new DomainException($"Cannot swap coils on a work order with status '{workOrder.Status}'.");
        }

        public static void ApplySwapCoil(WorkOrder workOrder, ApplicationUser user, InventoryItem newCoil, CoilSwapReason reason, string? notes, IClock clock)
        {
            // End the current usage
            if (workOrder.ActiveCoilUsage is not null)
            {
                workOrder.ActiveCoilUsage.EndedAt = clock.UtcNow;
            }

            // Start the new usage
            var newUsage = new WorkOrderCoilUsage
            {
                WorkOrderId = workOrder.Id,
                Sequence = (workOrder.CoilUsages.Any() ? workOrder.CoilUsages.Max(u => u.Sequence) : 0) + 1,
                CoilInventoryId = newCoil.Id,
                CoilTagNumber = newCoil.TagNumber ?? "N/A",
                CoilItemId = newCoil.ItemId,
                CoilDescription = newCoil.Description,
                StartWeightLbs = newCoil.Snapshot,
                FromLocation = newCoil.Location,
                StartedAt = clock.UtcNow,
                Reason = reason,
                Notes = notes
            };
            workOrder.CoilUsages.Add(newUsage);
            workOrder.ActiveCoilUsage = newUsage;

            workOrder.LastUpdatedBy = user.UserName;
            workOrder.LastUpdatedDate = clock.UtcNow;
        }
    }
}
