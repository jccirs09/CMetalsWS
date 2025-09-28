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

        public async Task<List<WorkOrder>> GenerateWorkOrdersForCreationAsync(WorkOrder masterWorkOrder, decimal coilWeight)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var generatedWorkOrders = new List<WorkOrder>();
            var coilRemainingWeight = coilWeight;

            var machine = await db.Machines.FindAsync(masterWorkOrder.MachineId) ?? throw new Exception("Machine not found");
            var customers = await db.Customers.AsNoTracking().ToListAsync();
            var taggedItem = await _inventoryService.GetByTagNumberAsync(masterWorkOrder.TagNumber) ?? throw new Exception("Tagged item not found");

            IQueryable<PickingListItem> query = db.PickingListItems
                .Include(p => p.PickingList).ThenInclude(p => p.Customer)
                .Where(p => p.Status == PickingLineStatus.Pending || p.Status == PickingLineStatus.InProgress);

            if (machine.Category == MachineCategory.Slitter)
            {
                query = query.Where(p => p.ItemId == taggedItem.ItemId);
            }
            else
            {
                var childItemIds = (await _itemRelationshipService.GetChildrenAsync(taggedItem.ItemId)).Select(c => c.ItemCode).ToList();
                if (!childItemIds.Any()) return generatedWorkOrders;
                query = query.Where(p => childItemIds.Contains(p.ItemId));
            }

            var availableItems = await query.OrderBy(p => p.PickingList.CustomerId).ThenBy(p => p.Id).ToListAsync();
            WorkOrder currentWorkOrder = null;

            foreach (var lineItem in availableItems)
            {
                var customer = customers.FirstOrDefault(c => c.Id == lineItem.PickingList.CustomerId) ?? throw new Exception("Customer not found for picking list item");
                var customerMaxSkidCapacity = customer.MaxSkidCapacity ?? machine.MaxSkidCapacity ?? 4000;
                var remainingOrderWeight = lineItem.Weight ?? 0;

                while (remainingOrderWeight > 0 && coilRemainingWeight > 0)
                {
                    if (currentWorkOrder == null || currentWorkOrder.Items.Any(i => i.CustomerId != customer.Id) || currentWorkOrder.Items.Sum(i => i.PlannedWeight ?? 0) >= customerMaxSkidCapacity)
                    {
                        currentWorkOrder = CreateNewWorkOrderFromMaster(masterWorkOrder);
                        generatedWorkOrders.Add(currentWorkOrder);
                    }

                    var currentSkidWeight = currentWorkOrder.Items.Sum(i => i.PlannedWeight ?? 0);
                    var availableSkidCapacity = customerMaxSkidCapacity - currentSkidWeight;

                    var constrainedWeight = Math.Min(availableSkidCapacity, Math.Min(coilRemainingWeight, remainingOrderWeight));
                    if (constrainedWeight <= 0) break;

                    var unitWeight = (lineItem.Weight ?? 0) / (lineItem.Quantity > 0 ? lineItem.Quantity : 1);
                    if (unitWeight <= 0) break;

                    var plannedQuantity = Math.Floor(constrainedWeight / unitWeight);
                    if (plannedQuantity == 0) break;
                    var plannedWeight = plannedQuantity * unitWeight;

                    string splitReason = null;
                    if (plannedWeight < remainingOrderWeight)
                    {
                        splitReason = (currentSkidWeight + plannedWeight >= customerMaxSkidCapacity) ? "skid-capacity" : "coil-capacity";
                    }

                    currentWorkOrder.Items.Add(new WorkOrderItem
                    {
                        PickingListItemId = lineItem.Id,
                        ItemCode = lineItem.ItemId,
                        Description = lineItem.ItemDescription,
                        SalesOrderNumber = lineItem.PickingList.SalesOrderNumber,
                        CustomerId = customer.Id,
                        CustomerName = customer.CustomerName,
                        CustomerMaxSkidCapacity = customerMaxSkidCapacity,
                        PlannedQuantity = plannedQuantity,
                        PlannedWeight = plannedWeight,
                        Status = WorkOrderItemStatus.Pending,
                        SplitReason = splitReason
                    });

                    remainingOrderWeight -= plannedWeight;
                    coilRemainingWeight -= plannedWeight;
                }
            }

            generatedWorkOrders.RemoveAll(wo => !wo.Items.Any());
            for (int i = 0; i < generatedWorkOrders.Count; i++)
            {
                generatedWorkOrders[i].WorkOrderSequence = i + 1;
                generatedWorkOrders[i].TotalWorkOrders = generatedWorkOrders.Count;
                generatedWorkOrders[i].IsMultiWorkOrder = generatedWorkOrders.Count > 1;
            }

            return generatedWorkOrders;
        }

        private WorkOrder CreateNewWorkOrderFromMaster(WorkOrder master)
        {
            return new WorkOrder
            {
                TagNumber = master.TagNumber,
                MachineId = master.MachineId,
                MachineCategory = master.MachineCategory,
                DueDate = master.DueDate,
                Instructions = master.Instructions,
                Priority = master.Priority,
                ParentItemId = master.ParentItemId,
                Items = new List<WorkOrderItem>()
            };
        }

        public async Task<List<WorkOrder>> CreateWorkOrdersAsync(List<WorkOrder> workOrders, string userId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
            if (!user.BranchId.HasValue) throw new InvalidOperationException("User has no default branch assigned.");

            var createdBy = user.UserName ?? user.Email ?? user.Id;

            foreach (var workOrder in workOrders)
            {
                workOrder.BranchId = user.BranchId.Value;
                workOrder.WorkOrderNumber = await GenerateWorkOrderNumber(workOrder.BranchId);
                workOrder.CreatedBy = createdBy;
                workOrder.CreatedDate = DateTime.UtcNow;
                workOrder.LastUpdatedBy = createdBy;
                workOrder.LastUpdatedDate = workOrder.CreatedDate;
                workOrder.Status = WorkOrderStatus.Pending;

                await AutoScheduleWorkOrder(db, workOrder);
                db.WorkOrders.Add(workOrder);
            }

            await db.SaveChangesAsync();

            foreach (var workOrder in workOrders)
            {
                await _auditEventService.CreateAuditEventAsync(workOrder.Id, TaskType.WorkOrder, AuditEventType.Create, userId, $"Work Order created (Part {workOrder.WorkOrderSequence}/{workOrder.TotalWorkOrders})");
                await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
            }

            return workOrders;
        }

        private async Task AutoScheduleWorkOrder(ApplicationDbContext db, WorkOrder workOrder)
        {
            if (!workOrder.ScheduledStartDate.HasValue)
            {
                workOrder.ScheduledStartDate = DateTime.UtcNow;
            };

            var machine = await db.Machines.FindAsync(workOrder.MachineId);
            if (machine == null) return;

            var totalWeight = workOrder.Items.Sum(i => i.PlannedWeight ?? 0);
            if (totalWeight == 0 || (machine.EstimatedLbsPerHour ?? 0) == 0)
            {
                workOrder.ScheduledEndDate = workOrder.ScheduledStartDate.Value.AddHours(1); // Default duration
                return;
            }

            var durationHours = (double)(totalWeight / machine.EstimatedLbsPerHour.Value);
            var duration = TimeSpan.FromHours(durationHours);

            var lastScheduledEnd = await db.WorkOrders
                .Where(wo => wo.MachineId == workOrder.MachineId &&
                             wo.ScheduledStartDate.HasValue &&
                             wo.ScheduledStartDate.Value.Date == workOrder.ScheduledStartDate.Value.Date &&
                             wo.Id != workOrder.Id)
                .MaxAsync(wo => (DateTime?)wo.ScheduledEndDate);

            workOrder.ScheduledStartDate = lastScheduledEnd ?? workOrder.ScheduledStartDate;
            workOrder.ScheduledEndDate = workOrder.ScheduledStartDate.Value.Add(duration);
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

            var dueDateChanged = existing.DueDate.Date != workOrder.DueDate.Date;

            // Update scalar properties
            existing.TagNumber = workOrder.TagNumber;
            existing.DueDate = workOrder.DueDate;
            existing.Instructions = workOrder.Instructions;
            existing.MachineId = workOrder.MachineId;
            existing.MachineCategory = workOrder.MachineCategory;
            existing.Priority = workOrder.Priority;
            existing.Shift = workOrder.Shift;
            existing.Status = workOrder.Status;
            existing.ActualLbs = workOrder.ActualLbs;
            existing.IsMultiWorkOrder = workOrder.IsMultiWorkOrder;
            existing.TotalWorkOrders = workOrder.TotalWorkOrders;
            existing.WorkOrderSequence = workOrder.WorkOrderSequence;
            existing.LastUpdatedBy = updatedBy;
            existing.LastUpdatedDate = DateTime.UtcNow;

            // Sync items
            var incomingItemIds = workOrder.Items.Select(i => i.Id).ToHashSet();
            var itemsToRemove = existing.Items.Where(i => i.Id != 0 && !incomingItemIds.Contains(i.Id)).ToList();
            db.WorkOrderItems.RemoveRange(itemsToRemove);

            foreach (var item in workOrder.Items)
            {
                var existingItem = item.Id == 0 ? null : existing.Items.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem == null)
                {
                    existing.Items.Add(item);
                }
                else
                {
                    existingItem.PlannedQuantity = item.PlannedQuantity;
                    existingItem.PlannedWeight = item.PlannedWeight;
                    existingItem.ProducedQuantity = item.ProducedQuantity;
                    existingItem.ProducedWeight = item.ProducedWeight;
                    existingItem.ManuallyAdjusted = item.ManuallyAdjusted;
                    existingItem.Status = item.Status;
                }
            }

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
                    foreach (var updatedItem in updatedItems)
                    {
                        var item = workOrder.Items.FirstOrDefault(i => i.Id == updatedItem.Id);
                        if (item != null)
                        {
                            item.ProducedQuantity = updatedItem.ProducedQuantity;
                            item.ProducedWeight = updatedItem.ProducedWeight;
                            item.Status = updatedItem.Status;
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

            if (workOrder.Status == WorkOrderStatus.Draft)
                workOrder.Status = WorkOrderStatus.Pending;

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