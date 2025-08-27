using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class WorkOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkOrderService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<List<WorkOrder>> GetAsync(int? branchId = null)
        {
            IQueryable<WorkOrder> query = _db.WorkOrders
                .Include(w => w.Items)
                .Include(w => w.Machine)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(w => w.BranchId == branchId.Value);

            return await query
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task<WorkOrder?> GetByIdAsync(int id)
        {
            return await _db.WorkOrders
                .Include(w => w.Items)
                .Include(w => w.Machine)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        // New overload: branch is taken from the user's default branch
        public async Task CreateAsync(WorkOrder workOrder, string userId)
        {
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

            _db.WorkOrders.Add(workOrder);
            await _db.SaveChangesAsync();
        }

        // Optional: keep the old signature but redirect to the enforced version
        public Task CreateAsync(WorkOrder workOrder, string createdBy, string userId)
            => CreateAsync(workOrder, userId);

        public async Task UpdateAsync(WorkOrder workOrder, string updatedBy)
        {
            var existing = await _db.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == workOrder.Id);

            if (existing is null) return;

            // Do not allow branch changes here. Keep existing.BranchId as-is.
            existing.TagNumber = workOrder.TagNumber;
            existing.DueDate = workOrder.DueDate;
            existing.Instructions = workOrder.Instructions;
            existing.MachineId = workOrder.MachineId;
            existing.MachineCategory = workOrder.MachineCategory;
            existing.Status = workOrder.Status;
            existing.LastUpdatedBy = updatedBy;
            existing.LastUpdatedDate = DateTime.UtcNow;

            existing.Items.Clear();
            foreach (var it in workOrder.Items)
                existing.Items.Add(it);

            await _db.SaveChangesAsync();
        }

        public async Task ScheduleAsync(int id, DateTime start, DateTime? end)
        {
            var workOrder = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id);
            if (workOrder is null) return;

            workOrder.ScheduledStartDate = start;
            workOrder.ScheduledEndDate = end ?? start;
            if (workOrder.Status == WorkOrderStatus.Draft)
                workOrder.Status = WorkOrderStatus.Pending;

            workOrder.LastUpdatedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

       

        private async Task<string> GenerateWorkOrderNumber(int branchId)
        {
            var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId);
            var branchCode = branch?.Code ?? "00";
            var next = await _db.WorkOrders.CountAsync(w => w.BranchId == branchId) + 1;
            return $"W{branchCode}{next:0000000}";
        }
        public async Task SetStatusAsync(int id, WorkOrderStatus status, string updatedBy)
        {
            var workOrder = await _db.WorkOrders
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workOrder is null) return;

            workOrder.Status = status;
            workOrder.LastUpdatedBy = updatedBy;
            workOrder.LastUpdatedDate = DateTime.UtcNow;

            // Update linked Picking List Items’ statuses to mirror work progress
            var pickingIds = workOrder.Items
                .Where(i => i.PickingListItemId != null)
                .Select(i => i.PickingListItemId!.Value)
                .Distinct()
                .ToList();

            if (pickingIds.Count > 0)
            {
                var plis = await _db.PickingListItems
                    .Where(p => pickingIds.Contains(p.Id))
                    .ToListAsync();

                // Map WorkOrderStatus -> PickingListStatus (adjust names to your enum if needed)
                foreach (var p in plis)
                {
                    switch (status)
                    {
                        case WorkOrderStatus.InProgress:
                            p.Status = PickingListStatus.InProgress;
                            break;
                        case WorkOrderStatus.Completed:
                            p.Status = PickingListStatus.ReadyToShip;
                            break;
                        case WorkOrderStatus.Canceled:
                            p.Status = PickingListStatus.Pending;
                            break;
                        default:
                            // no change for Draft/Pending
                            break;
                    }
                }

                // Optional: bump Loads’ ReadyDate affected by these picking items
                var loadService = new LoadService(_db);
                await loadService.RecalculateLoadsForPickingItemsAsync(pickingIds);
            }

            await _db.SaveChangesAsync();
        }
    }
}
