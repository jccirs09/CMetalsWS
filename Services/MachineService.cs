using CMetalsWS.Data;
using CMetalsWS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class MachineService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly WorkOrderService _workOrderService;
        private readonly PickingListService _pickingListService;

        public MachineService(IDbContextFactory<ApplicationDbContext> dbContextFactory, WorkOrderService workOrderService, PickingListService pickingListService)
        {
            _dbContextFactory = dbContextFactory;
            _workOrderService = workOrderService;
            _pickingListService = pickingListService;
        }

        public async Task<List<Machine>> GetMachinesAsync()
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Machines
                .Include(m => m.Branch)
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<List<Machine>> GetMachinesAsync(int branchId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Machines
                .Where(m => m.BranchId == branchId)
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Machine?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Machines.FindAsync(id);
        }

        public async Task CreateAsync(Machine model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Machines.Add(model);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Machine model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var local = db.Machines.Local.FirstOrDefault(x => x.Id == model.Id);
            if (local is not null)
                db.Entry(local).State = EntityState.Detached;

            db.Attach(model);
            db.Entry(model).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var entity = await db.Machines.FindAsync(id);
            if (entity != null)
            {
                db.Machines.Remove(entity);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<MachineDailyStatusDto>> GetMachineDailyStatusAsync(DateTime date)
        {
            var machines = await GetMachinesAsync();
            var workOrders = await _workOrderService.GetByDateAsync(date);
            var sheetItems = await _pickingListService.GetSheetPullingQueueAsync();
            var coilItems = await _pickingListService.GetCoilPullingQueueAsync();
            var pickingListItems = sheetItems.Concat(coilItems)
                .Where(i => i.ScheduledProcessingDate?.Date == date.Date)
                .ToList();

            var result = new List<MachineDailyStatusDto>();

            foreach (var machine in machines)
            {
                var now = DateTime.Now;
                var currentWorkOrder = workOrders.FirstOrDefault(wo => wo.MachineId == machine.Id && wo.ScheduledStartDate <= now && wo.ScheduledEndDate >= now);
                var currentPickingListItem = pickingListItems.FirstOrDefault(pi => pi.MachineId == machine.Id && pi.ScheduledProcessingDate <= now && (pi.ScheduledProcessingDate ?? DateTime.MinValue).AddHours(1) >= now);

                var status = "Idle";
                string? currentTask = null;

                if (currentWorkOrder != null)
                {
                    status = "Running";
                    currentTask = currentWorkOrder.WorkOrderNumber;
                }
                else if (currentPickingListItem != null)
                {
                    status = "Running";
                    currentTask = currentPickingListItem.ItemDescription;
                }

                result.Add(new MachineDailyStatusDto
                {
                    MachineId = machine.Id,
                    MachineName = machine.Name,
                    MachineType = machine.Category.ToString(),
                    Status = status,
                    CurrentWorkOrder = currentTask
                });
            }

            return result;
        }
    }
}
