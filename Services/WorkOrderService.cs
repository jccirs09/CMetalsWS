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

        public WorkOrderService(IDbContextFactory<ApplicationDbContext> dbContextFactory, UserManager<ApplicationUser> userManager, IHubContext<ScheduleHub> hubContext, PickingListService pickingListService)
        {
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
            _hubContext = hubContext;
            _pickingListService = pickingListService;
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

            var createdBy = user.UserName ?? user.Email ?? user.Id;
            workOrder.CreatedBy = createdBy;
            workOrder.CreatedDate = DateTime.UtcNow;
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

            workOrder.ScheduledStartDate = start;
            workOrder.ScheduledEndDate = end ?? start;
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

            foreach(var item in workOrder.Items)
            {
                var existingItem = existing.Items.FirstOrDefault(i => i.Id == item.Id);
                if(existingItem != null)
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

                foreach (var grpId in plis.Select(p => p.PickingListId).Distinct())
                    await _pickingListService.UpdatePickingListStatusAsync(grpId);
            }

            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("WorkOrderUpdated", workOrder.Id);
        }
    }
}
