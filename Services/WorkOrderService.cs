using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using CMetalsWS.Domain;
using CMetalsWS.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ScheduleHub> _hubContext;
        private readonly IPickingListStatusUpdater _pickingListUpdater;
        private readonly IClock _clock;

        public WorkOrderService(IDbContextFactory<ApplicationDbContext> dbContextFactory, UserManager<ApplicationUser> userManager, IHubContext<ScheduleHub> hubContext, IPickingListStatusUpdater pickingListUpdater, IClock clock)
        {
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
            _hubContext = hubContext;
            _pickingListUpdater = pickingListUpdater;
            _clock = clock;
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

        public async Task<WorkOrder?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.WorkOrders
                .Include(w => w.Items)
                .Include(w => w.Machine)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task CreateAsync(WorkOrder workOrder, string userId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");
            if (!user.BranchId.HasValue)
                throw new InvalidOperationException("User has no default branch assigned.");

            workOrder.BranchId = user.BranchId.Value;
            workOrder.WorkOrderNumber = await GenerateWorkOrderNumber(workOrder.BranchId);

            if (workOrder.CoilInventoryId.HasValue)
            {
                var coil = await db.InventoryItems.FindAsync(workOrder.CoilInventoryId.Value)
                    ?? throw new InvalidOperationException("Coil inventory item not found on WO creation.");
                WorkOrderRules.ApplyCoilSnapshot(workOrder, coil, _clock);
            }

            var createdBy = user.UserName ?? user.Email ?? user.Id;
            workOrder.CreatedBy = createdBy;
            workOrder.CreatedDate = _clock.UtcNow;
            workOrder.LastUpdatedBy = createdBy;
            workOrder.LastUpdatedDate = workOrder.CreatedDate;

            if (workOrder.Status == 0)
                workOrder.Status = WorkOrderStatus.Draft;

            // Auto-scheduling logic
            await AutoScheduleWorkOrder(db, workOrder);

            db.WorkOrders.Add(workOrder);
            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }

        private (TimeOnly Start, TimeOnly End) GetBranchWorkingHours(ApplicationDbContext db, int branchId)
        {
            // Per user instruction, hard-coded for now.
            // This would ideally come from the Branch entity in the database.
            return (new TimeOnly(5, 0), new TimeOnly(23, 59)); // 5 AM to 11:59 PM
        }

        private async Task AutoScheduleWorkOrder(ApplicationDbContext db, WorkOrder workOrder)
        {
            var workingHours = GetBranchWorkingHours(db, workOrder.BranchId);

            // Find the latest end time for any existing work order on the same machine and day.
            var lastScheduledEnd = await db.WorkOrders
                .Where(wo => wo.MachineId == workOrder.MachineId &&
                             wo.DueDate.Date == workOrder.DueDate.Date)
                .MaxAsync(wo => (DateTime?)wo.ScheduledEndDate); // Use nullable for MaxAsync to handle empty sets

            DateTime nextStartTime;

            if (lastScheduledEnd.HasValue)
            {
                // Start after the last one ends
                nextStartTime = lastScheduledEnd.Value;
            }
            else
            {
                // Or start at the beginning of the working day
                nextStartTime = workOrder.DueDate.Date + workingHours.Start.ToTimeSpan();
            }

            workOrder.ScheduledStartDate = nextStartTime;
            workOrder.ScheduledEndDate = nextStartTime.AddHours(1);
        }

        public Task CreateAsync(WorkOrder workOrder, string createdBy, string userId)
            => CreateAsync(workOrder, userId);

        public async Task UpdateAsync(WorkOrder workOrder, string updatedBy)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var existing = await db.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == workOrder.Id);

            if (existing is null) return;

            existing.TagNumber = workOrder.TagNumber;
            var dueDateChanged = existing.DueDate.Date != workOrder.DueDate.Date;
            existing.DueDate = workOrder.DueDate;
            existing.Instructions = workOrder.Instructions;
            existing.MachineId = workOrder.MachineId;
            existing.MachineCategory = workOrder.MachineCategory;
            existing.Status = workOrder.Status;
            existing.LastUpdatedBy = updatedBy;
            existing.LastUpdatedDate = DateTime.UtcNow;

            var incomingItemIds = workOrder.Items.Select(i => i.Id).ToHashSet();
            var itemsToRemove = existing.Items.Where(i => !incomingItemIds.Contains(i.Id)).ToList();
            db.WorkOrderItems.RemoveRange(itemsToRemove);

            foreach (var item in workOrder.Items)
            {
                var existingItem = existing.Items.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem == null)
                {
                    existing.Items.Add(item);
                }
                else
                {
                    existingItem.ItemCode = item.ItemCode;
                    existingItem.Description = item.Description;
                    existingItem.SalesOrderNumber = item.SalesOrderNumber;
                    existingItem.CustomerName = item.CustomerName;
                    existingItem.OrderQuantity = item.OrderQuantity;
                    existingItem.OrderWeight = item.OrderWeight;
                    existingItem.Width = item.Width;
                    existingItem.Length = item.Length;
                    existingItem.ProducedQuantity = item.ProducedQuantity;
                    existingItem.ProducedWeight = item.ProducedWeight;
                    existingItem.Unit = item.Unit;
                    existingItem.Location = item.Location;
                    existingItem.PickingListItemId = item.PickingListItemId;
                }
            }

            if (dueDateChanged)
            {
                await AutoScheduleWorkOrder(db, existing);
            }

            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }

        public async Task ScheduleAsync(int id, DateTime start, DateTime? end)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var workOrder = await db.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (workOrder is null) return;

            var originalDuration = workOrder.ScheduledEndDate - workOrder.ScheduledStartDate;
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

            await db.SaveChangesAsync();

            foreach (var grpId in plis.Select(p => p.PickingListId).Distinct())
                await _pickingListUpdater.UpdatePickingListStatusAsync(grpId, PickingListStatus.Awaiting);
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }

        private async Task<string> GenerateWorkOrderNumber(int branchId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId);
            var branchCode = branch?.Code ?? "00";
            var next = await db.WorkOrders.CountAsync(w => w.BranchId == branchId) + 1;
            return $"W{branchCode}{next:0000000}";
        }

        public async Task MarkWorkOrderCompleteAsync(WorkOrder workOrder, string updatedBy)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var existing = await db.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == workOrder.Id);

            if (existing is null) return;

            foreach (var item in workOrder.Items)
            {
                var existingItem = existing.Items.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem != null)
                {
                    existingItem.ProducedQuantity = item.ProducedQuantity;
                    existingItem.ProducedWeight = item.ProducedWeight;
                }
            }

            await SetStatusAsync(workOrder.Id, WorkOrderStatus.Completed, updatedBy);
        }

        public async Task SetStatusAsync(int id, WorkOrderStatus status, string updatedBy)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var workOrder = await db.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workOrder is null) return;

            workOrder.Status = status;
            workOrder.LastUpdatedBy = updatedBy;
            workOrder.LastUpdatedDate = DateTime.UtcNow;

            var lineIds = workOrder.Items
                .Where(i => i.PickingListItemId != null)
                .Select(i => i.PickingListItemId!.Value)
                .ToList();

            if (lineIds.Count > 0)
            {
                var plis = await db.PickingListItems
                    .Where(p => lineIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var p in plis)
                {
                    switch (status)
                    {
                        case WorkOrderStatus.InProgress:
                            p.Status = PickingLineStatus.InProgress;
                            break;
                        case WorkOrderStatus.Completed:
                            p.Status = PickingLineStatus.Completed;
                            break;
                        case WorkOrderStatus.Canceled:
                            p.Status = PickingLineStatus.Canceled;
                            break;
                        default:
                            break;
                    }
                }

                // Determine the new aggregate status for the picking list
                var newPickingListStatus = status switch
                {
                    WorkOrderStatus.InProgress => PickingListStatus.InProgress,
                    WorkOrderStatus.Completed => PickingListStatus.Completed,
                    WorkOrderStatus.Canceled => PickingListStatus.Pending, // Revert to pending
                    _ => PickingListStatus.Pending // Default case
                };

                await db.SaveChangesAsync();

                foreach (var grpId in plis.Select(p => p.PickingListId).Distinct())
                    await _pickingListUpdater.UpdatePickingListStatusAsync(grpId, newPickingListStatus);
            }
            else
            {
                await db.SaveChangesAsync();
            }
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }

        public async Task StartWorkOrderAsync(int workOrderId, string userId)
        {
            var db = await _dbContextFactory.CreateDbContextAsync();
            var workOrder = await db.WorkOrders
                .Include(wo => wo.CoilUsages)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId)
                ?? throw new KeyNotFoundException("Work order not found.");

            WorkOrderRules.ValidateCanStart(workOrder);

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            InventoryItem? coil = null;
            if (workOrder.CoilInventoryId.HasValue)
            {
                coil = await db.InventoryItems.FindAsync(workOrder.CoilInventoryId.Value);
            }

            WorkOrderRules.ApplyStart(workOrder, user, coil, _clock);

            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }

        public async Task PauseWorkOrderAsync(int workOrderId, string userId)
        {
            var db = await _dbContextFactory.CreateDbContextAsync();
            var workOrder = await db.WorkOrders.FindAsync(workOrderId)
                ?? throw new KeyNotFoundException("Work order not found.");

            WorkOrderRules.ValidateCanPause(workOrder);

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            WorkOrderRules.ApplyPause(workOrder, user, _clock);

            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }

        public async Task ResumeWorkOrderAsync(int workOrderId, string userId)
        {
            var db = await _dbContextFactory.CreateDbContextAsync();
            var workOrder = await db.WorkOrders.FindAsync(workOrderId)
                ?? throw new KeyNotFoundException("Work order not found.");

            WorkOrderRules.ValidateCanResume(workOrder);

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            WorkOrderRules.ApplyResume(workOrder, user, _clock);

            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }

        public async Task CompleteWorkOrderAsync(int workOrderId, string userId)
        {
            var db = await _dbContextFactory.CreateDbContextAsync();
            var workOrder = await db.WorkOrders
                .Include(wo => wo.ActiveCoilUsage)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId)
                ?? throw new KeyNotFoundException("Work order not found.");

            WorkOrderRules.ValidateCanComplete(workOrder);

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            WorkOrderRules.ApplyComplete(workOrder, user, _clock);

            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }

        public async Task SwapCoilAsync(int workOrderId, int newCoilInventoryId, CoilSwapReason reason, string? notes, string userId)
        {
            var db = await _dbContextFactory.CreateDbContextAsync();
            var workOrder = await db.WorkOrders
                .Include(wo => wo.CoilUsages)
                .Include(wo => wo.ActiveCoilUsage)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId)
                ?? throw new KeyNotFoundException("Work order not found.");

            WorkOrderRules.ValidateCanSwapCoil(workOrder);

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            var newCoil = await db.InventoryItems.FindAsync(newCoilInventoryId)
                ?? throw new KeyNotFoundException("New coil inventory item not found.");

            WorkOrderRules.ApplySwapCoil(workOrder, user, newCoil, reason, notes, _clock);

            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }
    }
}