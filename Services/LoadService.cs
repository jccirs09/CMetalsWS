using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class LoadService
    {
        private readonly ApplicationDbContext _db;
        public LoadService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Load>> GetLoadsAsync(int? branchId = null)
        {
            IQueryable<Load> query = _db.Loads
                .Include(l => l.Truck)
                .Include(l => l.Items)
                .ThenInclude(i => i.WorkOrder);

            if (branchId.HasValue)
                query = query.Where(l => l.BranchId == branchId.Value);

            return await query.OrderByDescending(l => l.ScheduledDate).ToListAsync();
        }

        public async Task<Load?> GetByIdAsync(int id)
        {
            return await _db.Loads
                .Include(l => l.Truck)
                .Include(l => l.Items)
                .ThenInclude(i => i.WorkOrder)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task CreateAsync(Load load)
        {
            load.LoadNumber = await GenerateLoadNumber(load.BranchId);
            _db.Loads.Add(load);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Load load)
        {
            var existing = await _db.Loads
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.Id == load.Id);
            if (existing is null) return;

            existing.Status = load.Status;
            existing.ScheduledDate = load.ScheduledDate;
            existing.TruckId = load.TruckId;
            existing.Items.Clear();
            foreach (var item in load.Items)
            {
                existing.Items.Add(item);
            }
            await _db.SaveChangesAsync();
        }

        private async Task<string> GenerateLoadNumber(int branchId)
        {
            var branchCode = (await _db.Branches.FindAsync(branchId))?.Code ?? "00";
            var count = await _db.Loads.CountAsync(l => l.BranchId == branchId) + 1;
            return $"L{branchCode}{count:000000}";
        }
    }
}
