using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class WorkOrderService
    {
        private readonly ApplicationDbContext _db;
        public WorkOrderService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<WorkOrder>> GetAsync(int? branchId = null)
        {
            var query = _db.WorkOrders
                           .Include(w => w.Machine)
                           .Include(w => w.Branch)
                           .Include(w => w.Items);
            if (branchId.HasValue)
                query = query.Where(w => w.BranchId == branchId.Value);
            return await query.ToListAsync();
        }

        public async Task<WorkOrder?> GetByIdAsync(int id)
        {
            return await _db.WorkOrders
                            .Include(w => w.Machine)
                            .Include(w => w.Branch)
                            .Include(w => w.Items)
                            .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task CreateAsync(WorkOrder order, string createdBy)
        {
            order.CreatedBy = createdBy;
            order.CreatedDate = DateTime.UtcNow;
            order.LastUpdatedBy = createdBy;
            order.LastUpdatedDate = DateTime.UtcNow;
            order.WorkOrderNumber = await GenerateNumber(order.BranchId);
            _db.WorkOrders.Add(order);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(WorkOrder order, string updatedBy)
        {
            var existing = await _db.WorkOrders
                                    .Include(w => w.Items)
                                    .FirstOrDefaultAsync(w => w.Id == order.Id);
            if (existing == null) return;

            existing.TagNumber = order.TagNumber;
            existing.MachineId = order.MachineId;
            existing.MachineCategory = order.MachineCategory;
            existing.DueDate = order.DueDate;
            existing.Instructions = order.Instructions;
            existing.Status = order.Status;
            existing.LastUpdatedBy = updatedBy;
            existing.LastUpdatedDate = DateTime.UtcNow;

            // Update items
            existing.Items.Clear();
            foreach (var item in order.Items)
            {
                existing.Items.Add(item);
            }

            await _db.SaveChangesAsync();
        }

        public async Task ScheduleAsync(int workOrderId, DateTime start, DateTime end)
        {
            var order = await _db.WorkOrders.FindAsync(workOrderId);
            if (order != null)
            {
                order.ScheduledStartDate = start;
                order.ScheduledEndDate = end;
                order.Status = WorkOrderStatus.Pending;
                await _db.SaveChangesAsync();
            }
        }

        private async Task<string> GenerateNumber(int branchId)
        {
            var branch = await _db.Branches.FindAsync(branchId);
            var prefix = branch?.Code ?? "00";
            var count = await _db.WorkOrders.CountAsync(w => w.BranchId == branchId) + 1;
            return $"W{prefix}{count:0000000}";
        }
    }
}
