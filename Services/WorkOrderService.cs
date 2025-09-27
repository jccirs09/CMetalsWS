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

        public async Task<(InventoryItem? ParentItem, List<PickingListItem> AvailableItems)> GetPickingListItemsForWorkOrderAsync(MachineCategory machineCategory, string tagNumber)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var taggedItem = await _inventoryService.GetByTagNumberAsync(tagNumber);
            if (taggedItem == null)
            {
                throw new Exception($"Tag '{tagNumber}' not found in inventory.");
            }

            IQueryable<PickingListItem> query = db.PickingListItems.Include(p => p.PickingList).ThenInclude(p => p.Customer);

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
                workOrder.Status = WorkOrderStatus.Draft;

            await AutoScheduleWorkOrder(db, workOrder);

            db.WorkOrders.Add(workOrder);
            await db.SaveChangesAsync();

            await _auditEventService.CreateAuditEventAsync(workOrder.Id, TaskType.WorkOrder, AuditEventType.Create, userId, "Work Order created.");
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);

            return workOrder;
        }

        private async Task AutoScheduleWorkOrder(ApplicationDbContext db, WorkOrder workOrder)
        {
            var machine = await db.Machines.FindAsync(workOrder.MachineId);
            if (machine == null) return;

            var totalWeight = workOrder.Items.Sum(i => i.OrderWeight ?? 0);
            if (totalWeight == 0 || machine.EstimatedLbsPerHour == 0)
            {
                workOrder.ScheduledEndDate = workOrder.ScheduledStartDate.AddHours(1); // Default duration
                return;
            }

            var durationHours = (double)(totalWeight / machine.EstimatedLbsPerHour);
            var duration = TimeSpan.FromHours(durationHours);

            var lastScheduledEnd = await db.WorkOrders
                .Where(wo => wo.MachineId == workOrder.MachineId &&
                             wo.ScheduledStartDate.Date == workOrder.ScheduledStartDate.Date &&
                             wo.Id != workOrder.Id)
                .MaxAsync(wo => (DateTime?)wo.ScheduledEndDate);

            workOrder.ScheduledStartDate = lastScheduledEnd ?? workOrder.ScheduledStartDate;
            workOrder.ScheduledEndDate = workOrder.ScheduledStartDate.Add(duration);
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
    }
}