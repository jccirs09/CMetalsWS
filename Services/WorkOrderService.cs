using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using CMetalsWS.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class WorkOrderService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ScheduleHub> _hubContext;
        private readonly PickingListService _pickingListService;
        private readonly ITaskAuditEventService _auditEventService;
        private readonly ItemRelationshipService _itemRelationshipService;
        private readonly InventoryService _inventoryService;
        private static readonly TimeSpan DefaultWorkOrderDuration = TimeSpan.FromHours(1);

        public WorkOrderService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            UserManager<ApplicationUser> userManager,
            IHubContext<ScheduleHub> hubContext,
            PickingListService pickingListService,
            ITaskAuditEventService auditEventService,
            ItemRelationshipService itemRelationshipService,
            InventoryService inventoryService)
        {
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
            _hubContext = hubContext;
            _pickingListService = pickingListService;
            _auditEventService = auditEventService;
            _itemRelationshipService = itemRelationshipService;
            _inventoryService = inventoryService;
        }

        public async Task<List<WorkOrder>> GetAsync(int? branchId = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            IQueryable<WorkOrder> query = db.WorkOrders
                .Include(w => w.Items)
                .Include(w => w.Machine)
                .Include(w => w.Branch)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(w => w.BranchId == branchId.Value);

            return await query
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task<WorkOrder?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.WorkOrders
                .Include(w => w.Items)
                    .ThenInclude(i => i.PickingListItem)
                .Include(w => w.Machine)
                .Include(w => w.Branch)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<(InventoryItem? ParentItem, List<PickingListItem> AvailableItems)> GetPickingListItemsForWorkOrderAsync(MachineCategory machineCategory, string tagNumber)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var taggedItem = await _inventoryService.GetByTagNumberAsync(tagNumber);
            if (taggedItem == null)
            {
                throw new Exception($"Tag '{tagNumber}' not found in inventory.");
            }

            IQueryable<PickingListItem> query = db.PickingListItems
                .Include(p => p.PickingList).ThenInclude(p => p.Customer)
                .Include(p => p.Machine);

            // Filter by the machine category selected in the first step.
            query = query.Where(p => p.Machine != null && p.Machine.Category == machineCategory);

            if (machineCategory == MachineCategory.Slitter)
            {
                query = query.Where(p => p.ItemId == taggedItem.ItemId);
            }
            else // CTL
            {
                // For CTL, the tagged item IS the parent coil. Find its children.
                var childItems = await _itemRelationshipService.GetChildrenAsync(taggedItem.ItemId);
                if (!childItems.Any())
                {
                    return (taggedItem, new List<PickingListItem>()); // No children, no items
                }
                var childItemIds = childItems.Select(c => c.ItemCode).ToList();

                query = query.Where(p => childItemIds.Contains(p.ItemId));
            }

            return (taggedItem, await query.AsNoTracking().ToListAsync());
        }

        public async Task<decimal> GetRemainingQuantityForPickingListItemAsync(int pickingListItemId)
        {
            using var db = _dbContextFactory.CreateDbContext();

            var pickingListItem = await db.PickingListItems
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == pickingListItemId);

            if (pickingListItem == null)
            {
                return 0;
            }

            var totalQuantity = pickingListItem.Quantity;

            var plannedQuantity = await db.WorkOrderItems
                .AsNoTracking()
                .Where(wi => wi.PickingListItemId == pickingListItemId)
                .SumAsync(wi => wi.OrderQuantity);

            var remainingQuantity = totalQuantity - (plannedQuantity ?? 0);

            return remainingQuantity > 0 ? remainingQuantity : 0;
        }

        public async Task<Dictionary<int, decimal>> GetRemainingQuantitiesForPickingListItemsAsync(IEnumerable<int> pickingListItemIds)
        {
            if (pickingListItemIds == null || !pickingListItemIds.Any())
            {
                return new Dictionary<int, decimal>();
            }

            using var db = _dbContextFactory.CreateDbContext();

            var pickingListItems = await db.PickingListItems
                .AsNoTracking()
                .Where(p => pickingListItemIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Quantity);

            var plannedQuantities = await db.WorkOrderItems
                .AsNoTracking()
                .Where(wi => wi.PickingListItemId.HasValue && pickingListItemIds.Contains(wi.PickingListItemId.Value))
                .GroupBy(wi => wi.PickingListItemId.Value)
                .Select(g => new { PickingListItemId = g.Key, TotalPlanned = g.Sum(wi => wi.OrderQuantity) })
                .ToDictionaryAsync(g => g.PickingListItemId, g => g.TotalPlanned ?? 0);

            var remainingQuantities = new Dictionary<int, decimal>();
            foreach (var id in pickingListItemIds)
            {
                var totalQuantity = pickingListItems.GetValueOrDefault(id, 0);
                var plannedQuantity = plannedQuantities.GetValueOrDefault(id, 0);
                var remaining = totalQuantity - plannedQuantity;
                remainingQuantities[id] = remaining > 0 ? remaining : 0;
            }

            return remainingQuantities;
        }

        public async Task<WorkOrder> CreateAsync(WorkOrder workOrder, string userId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");
            if (!user.BranchId.HasValue)
                throw new InvalidOperationException("User has no default branch assigned.");

            workOrder.BranchId = user.BranchId.Value;
            workOrder.WorkOrderNumber = await GenerateWorkOrderNumber(workOrder.BranchId);

            var createdBy = user.UserName ?? user.Email ?? user.Id;
            workOrder.CreatedBy = createdBy;
            workOrder.CreatedDate = DateTime.UtcNow;
            workOrder.LastUpdatedBy = createdBy;
            workOrder.LastUpdatedDate = workOrder.CreatedDate;

            if (workOrder.Status == 0)
                workOrder.Status = WorkOrderStatus.Pending;

            await ValidateSplitQuantities(db, workOrder);
            await AutoScheduleWorkOrder(db, workOrder);

            db.WorkOrders.Add(workOrder);
            await db.SaveChangesAsync();

            await _auditEventService.CreateAuditEventAsync(workOrder.Id, TaskType.WorkOrder, AuditEventType.Create, userId, "Work Order created.");
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);

            return workOrder;
        }

        private async Task AutoScheduleWorkOrder(ApplicationDbContext db, WorkOrder workOrder)
        {
            // Must have a picked date.
            if (!workOrder.ScheduledStartDate.HasValue)
                return;

            var day = workOrder.ScheduledStartDate.Value.Date;

            // Need machine for filtering; branch for shift time.
            var machine = await db.Machines.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == workOrder.MachineId);
            if (machine is null) return;

            var branch = await db.Branches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == workOrder.BranchId);

            // Duration: honor explicit end if provided; else default.
            var duration = (workOrder.ScheduledEndDate.HasValue && workOrder.ScheduledStartDate.HasValue)
                ? (workOrder.ScheduledEndDate.Value - workOrder.ScheduledStartDate.Value)
                : DefaultWorkOrderDuration;

            if (duration <= TimeSpan.Zero)
                duration = DefaultWorkOrderDuration;

            // Earliest shift start for that date:
            // 1) Branch.StartTime if set
            // 2) Earliest Shift.StartTime for the branch
            // 3) Fallback 06:00
            TimeSpan earliestSpan;
            if (branch?.StartTime is TimeOnly brStart)
            {
                earliestSpan = brStart.ToTimeSpan();
            }
            else
            {
                var earliestShift = await db.Set<Shift>()
                    .AsNoTracking()
                    .Where(s => s.BranchId == workOrder.BranchId)
                    .OrderBy(s => s.StartTime)
                    .Select(s => (TimeSpan?)s.StartTime.ToTimeSpan())
                    .FirstOrDefaultAsync();

                earliestSpan = earliestShift ?? TimeSpan.FromHours(6);
            }

            var dayEarliest = day.Add(earliestSpan);

            // Latest scheduled end on that machine for the same date (excluding current WO)
            var lastEnd = await db.WorkOrders
                .Where(wo =>
                    wo.MachineId == workOrder.MachineId &&
                    wo.Id != workOrder.Id &&
                    wo.ScheduledStartDate.HasValue &&
                    wo.ScheduledEndDate.HasValue &&
                    wo.ScheduledStartDate.Value.Date == day)
                .MaxAsync(wo => (DateTime?)wo.ScheduledEndDate);

            // Your rule:
            // - none exist => start at earliest branch shift for selected date
            // - exists     => start at last end + 1h
            var start = lastEnd.HasValue ? lastEnd.Value.AddHours(1) : dayEarliest;

            // Don’t start before earliest shift (if user picked 00:00 etc.)
            if (start < dayEarliest)
                start = dayEarliest;

            workOrder.ScheduledStartDate = start;
            workOrder.ScheduledEndDate   = start.Add(duration);
        }

        public async Task UpdateAsync(WorkOrder workOrder, string userId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");
            var updatedBy = user.UserName ?? user.Email ?? user.Id;

            var existing = await db.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == workOrder.Id);

            if (existing is null) return;

            var dueDateChanged = !Nullable.Equals(existing.DueDate?.Date, workOrder.DueDate?.Date);

            existing.TagNumber = workOrder.TagNumber;
            existing.DueDate = workOrder.DueDate;
            existing.Instructions = workOrder.Instructions;
            existing.MachineId = workOrder.MachineId;
            existing.MachineCategory = workOrder.MachineCategory;
            existing.Priority = workOrder.Priority;
            existing.Shift = workOrder.Shift;
            existing.Status = workOrder.Status;
            existing.LastUpdatedBy = updatedBy;
            existing.LastUpdatedDate = DateTime.UtcNow;

            var incomingItemIds = workOrder.Items.Select(i => i.Id).ToHashSet();
            var itemsToRemove = existing.Items.Where(i => i.Id != 0 && !incomingItemIds.Contains(i.Id)).ToList();
            db.WorkOrderItems.RemoveRange(itemsToRemove);

            foreach (var item in workOrder.Items)
            {
                var existingItem = item.Id == 0 ? null : existing.Items.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem == null)
                {
                    // This is a new item, add it to the tracked collection
                    existing.Items.Add(item);
                }
                else
                {
                    // This is an existing item, update its values
                    db.Entry(existingItem).CurrentValues.SetValues(item);
                }
            }

            await ValidateSplitQuantities(db, existing);

            if (dueDateChanged)
            {
                await AutoScheduleWorkOrder(db, existing);
            }

            await db.SaveChangesAsync();
            await _auditEventService.CreateAuditEventAsync(workOrder.Id, TaskType.WorkOrder, AuditEventType.Update, userId, "Work Order details updated.");
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }

        public async Task StartWorkOrderAsync(int workOrderId, string userId)
        {
            await SetStatusInternalAsync(workOrderId, WorkOrderStatus.InProgress, userId, AuditEventType.Start);
        }

        public async Task PauseWorkOrderAsync(int workOrderId, string userId, string notes)
        {
            await SetStatusInternalAsync(workOrderId, WorkOrderStatus.Awaiting, userId, AuditEventType.Pause, notes);
        }

        public async Task ResumeWorkOrderAsync(int workOrderId, string userId)
        {
            await SetStatusInternalAsync(workOrderId, WorkOrderStatus.InProgress, userId, AuditEventType.Resume);
        }

        public async Task CompleteWorkOrderAsync(int workOrderId, string userId, IEnumerable<WorkOrderItem> updatedItems)
        {
            await SetStatusInternalAsync(workOrderId, WorkOrderStatus.Completed, userId, AuditEventType.Complete, updatedItems: updatedItems);
        }

    private async Task ValidateSplitQuantities(ApplicationDbContext db, WorkOrder workOrder)
    {
        var allPickingListItemIds = workOrder.Items
            .Where(i => i.PickingListItemId.HasValue)
            .Select(i => i.PickingListItemId.Value)
            .Distinct()
            .ToList();

        if (!allPickingListItemIds.Any()) return;

        var originalPickingListItems = await db.PickingListItems
            .AsNoTracking()
            .Where(p => allPickingListItemIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Quantity);

        var otherWorkOrderItemsQuery = db.WorkOrderItems
            .AsNoTracking()
            .Where(i => i.PickingListItemId.HasValue &&
                        allPickingListItemIds.Contains(i.PickingListItemId.Value));

        if (workOrder.Id != 0)
        {
            otherWorkOrderItemsQuery = otherWorkOrderItemsQuery.Where(i => i.WorkOrderId != workOrder.Id);
        }

        var plannedQuantitiesInOtherWOs = await otherWorkOrderItemsQuery
            .GroupBy(i => i.PickingListItemId.Value)
            .Select(g => new { PickingListItemId = g.Key, Total = g.Sum(i => i.OrderQuantity) ?? 0 })
            .ToDictionaryAsync(x => x.PickingListItemId, x => x.Total);

        foreach (var pliId in allPickingListItemIds)
        {
            var quantityInCurrentWO = workOrder.Items
                .Where(i => i.PickingListItemId == pliId)
                .Sum(i => i.OrderQuantity ?? 0);

            var quantityInOtherWOs = plannedQuantitiesInOtherWOs.GetValueOrDefault(pliId, 0);

            var totalProposedQuantity = quantityInCurrentWO + quantityInOtherWOs;

            var originalQuantity = originalPickingListItems.GetValueOrDefault(pliId, 0);

            if (totalProposedQuantity > originalQuantity)
            {
                var pli = await db.PickingListItems.FindAsync(pliId);
                throw new InvalidOperationException($"Cannot save. Total quantity for Picking List Item '{pli?.ItemId}' ({totalProposedQuantity}) would exceed available quantity ({originalQuantity}).");
            }
        }
    }

        private async Task SetStatusInternalAsync(int workOrderId, WorkOrderStatus status, string userId, AuditEventType eventType, string? notes = null, IEnumerable<WorkOrderItem>? updatedItems = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var workOrder = await db.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == workOrderId);

            if (workOrder is null) return;

            var user = await _userManager.FindByIdAsync(userId);
            if(user is null) throw new Exception("User not found for status update");

            workOrder.Status = status;
            workOrder.LastUpdatedBy = user.UserName;
            workOrder.LastUpdatedDate = DateTime.UtcNow;

            if (eventType == AuditEventType.Start)
            {
                workOrder.ActualStartDate = DateTime.UtcNow;
                workOrder.Operator = user.UserName;
            }
            else if (eventType == AuditEventType.Complete)
            {
                workOrder.ActualEndDate = DateTime.UtcNow;
                if (updatedItems != null)
                {
                    var matchedServerItemIds = new HashSet<int>();

                    foreach (var updatedItem in updatedItems)
                    {
                        WorkOrderItem serverItem = null;

                        // Primary match by ID
                        if (updatedItem.Id != 0)
                        {
                            serverItem = workOrder.Items.FirstOrDefault(i => i.Id == updatedItem.Id);
                        }

                        // Fallback match for new items
                        if (serverItem == null)
                        {
                            serverItem = workOrder.Items
                                .Where(s => !matchedServerItemIds.Contains(s.Id)) // Only search unmatched items
                                .FirstOrDefault(s =>
                                    s.PickingListItemId == updatedItem.PickingListItemId &&
                                    s.OrderQuantity == updatedItem.OrderQuantity &&
                                    (Math.Abs((s.OrderWeight ?? 0) - (updatedItem.OrderWeight ?? 0)) / Math.Max(1, updatedItem.OrderWeight ?? 1)) <= 0.005m
                                );
                        }

                        if (serverItem != null)
                        {
                            serverItem.ProducedQuantity = updatedItem.ProducedQuantity;
                            serverItem.ProducedWeight = updatedItem.ProducedWeight;
                            serverItem.Status = updatedItem.Status;
                            matchedServerItemIds.Add(serverItem.Id);
                        }
                    }
                }
            }

            var lineIds = workOrder.Items
                .Where(i => i.PickingListItemId != null)
                .Select(i => i.PickingListItemId!.Value)
                .ToList();

            if (lineIds.Any())
            {
                var plis = await db.PickingListItems
                    .Where(p => lineIds.Contains(p.Id))
                    .ToListAsync();

                var newPickingStatus = status switch
                {
                    WorkOrderStatus.InProgress => PickingLineStatus.InProgress,
                    WorkOrderStatus.Awaiting => PickingLineStatus.InProgress,
                    WorkOrderStatus.Completed => PickingLineStatus.Completed,
                    WorkOrderStatus.Canceled => PickingLineStatus.Canceled,
                    _ => (PickingLineStatus?)null
                };

                if (newPickingStatus.HasValue)
                {
                    foreach (var p in plis)
                    {
                        p.Status = newPickingStatus.Value;
                    }

                    foreach (var grpId in plis.Select(p => p.PickingListId).Distinct())
                    {
                        await _pickingListService.UpdatePickingListStatusAsync(grpId);
                    }
                }
            }

            await db.SaveChangesAsync();
            await _auditEventService.CreateAuditEventAsync(workOrderId, TaskType.WorkOrder, eventType, userId, notes);
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrderId);
        }

        private async Task<string> GenerateWorkOrderNumber(int branchId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId);
            var branchCode = branch?.Code ?? "00";
            var next = await db.WorkOrders.CountAsync(w => w.BranchId == branchId) + 1;
            return $"W{branchCode}{next:0000000}";
        }

        public async Task<List<WorkOrder>> GetByCategoryAsync(MachineCategory category, int? branchId = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            IQueryable<WorkOrder> query = db.WorkOrders
                .Include(w => w.Items)
                .Include(w => w.Machine)
                .Where(w => w.MachineCategory == category)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(w => w.BranchId == branchId.Value);

            return await query
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task ScheduleAsync(int id, DateTime start, DateTime? end)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var workOrder = await db.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (workOrder is null) return;

            var originalDuration = (workOrder.ScheduledEndDate.HasValue && workOrder.ScheduledStartDate.HasValue)
                ? workOrder.ScheduledEndDate.Value - workOrder.ScheduledStartDate.Value
                : TimeSpan.FromHours(1); // Default duration
            workOrder.ScheduledStartDate = start;
            workOrder.ScheduledEndDate = end ?? (start + originalDuration);


            workOrder.LastUpdatedDate = DateTime.UtcNow;

            var lineIds = workOrder.Items
                .Where(i => i.PickingListItemId != null)
                .Select(i => i.PickingListItemId!.Value)
                .ToList();
            var plis = await db.PickingListItems
                .Where(p => lineIds.Contains(p.Id))
                .ToListAsync();
            foreach (var li in plis)
                li.Status = PickingLineStatus.WorkOrder;

            foreach (var grpId in plis.Select(p => p.PickingListId).Distinct())
                await _pickingListService.UpdatePickingListStatusAsync(grpId);

            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }
    }
}