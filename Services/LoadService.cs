using System;
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
        public LoadService(ApplicationDbContext db) => _db = db;

        public async Task<List<Load>> GetLoadsAsync(int? branchId = null, DateTime? onlyDate = null)
        {
            IQueryable<Load> query = _db.Loads
                .Include(l => l.Truck)
                .Include(l => l.Items)
                    .ThenInclude(i => i.PickingList)
                        .ThenInclude(p => p!.Customer);

            if (branchId.HasValue)
                query = query.Where(l => l.BranchId == branchId.Value);

            if (onlyDate.HasValue)
            {
                var d = onlyDate.Value.Date;
                query = query.Where(l =>
                    (l.ScheduledStart.HasValue && l.ScheduledStart.Value.Date == d) ||
                    (l.ScheduledDate.HasValue && l.ScheduledDate.Value.Date == d));
            }

            return await query.OrderByDescending(l => l.ScheduledStart).ToListAsync();
        }

        public async Task<Load?> GetByIdAsync(int id)
        {
            return await _db.Loads
                .Include(l => l.Truck)
                .Include(l => l.Items).ThenInclude(i => i.PickingList).ThenInclude(p => p!.Customer)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task CreateAsync(Load load)
        {
            load.LoadNumber = await GenerateLoadNumber(load.BranchId);
            await RecalculateReadyDateFromWorkOrdersAsync(load);
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
            existing.ScheduledStart = load.ScheduledStart;
            existing.ScheduledEnd = load.ScheduledEnd;
            existing.ScheduledDate = load.ScheduledDate;
            existing.TruckId = load.TruckId;

            existing.Items.Clear();
            foreach (var item in load.Items)
                existing.Items.Add(item);

            await RecalculateReadyDateFromWorkOrdersAsync(existing);
            await _db.SaveChangesAsync();
        }

        public async Task ScheduleAsync(int loadId, DateTime start, DateTime? end = null)
        {
            var load = await _db.Loads
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.Id == loadId);
            if (load is null) return;

            var newEnd = end ?? start.AddHours(1);
            if (newEnd < start) newEnd = start;

            load.ScheduledStart = start;
            load.ScheduledEnd = newEnd;

            if (load.Status == LoadStatus.Pending)
                load.Status = LoadStatus.Scheduled;

            await RecalculateReadyDateFromWorkOrdersAsync(load);
            await _db.SaveChangesAsync();
        }

        // Computes a grouping key for routing from the customers on this load’s picking list items
        public static string GetLoadRegionCode(Load load)
        {
            var codes = load.Items
                .Where(i => i.PickingList?.Customer != null)
                .Select(i => i.PickingList!.Customer!.LocationCode)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (codes.Count == 1) return codes[0]!;
            if (codes.Count > 1) return "MULTI";
            return "UNSET";
        }

        public async Task RecalculateReadyDateFromWorkOrdersAsync(Load load)
        {
            var pickingItemIds = await _db.LoadItems
                .Where(li => li.LoadId == load.Id)
                .Select(li => li.PickingListItemId)
                .Where(id => id != null)
                .Select(id => id!.Value)
                .ToListAsync();

            if (pickingItemIds.Count == 0)
            {
                load.ReadyDate = null;
                return;
            }

            var relatedWorkOrders = await _db.WorkOrders
                .Where(wo => wo.Items.Any(wi => wi.PickingListItemId != null && pickingItemIds.Contains(wi.PickingListItemId.Value)))
                .Select(wo => new { EndOrStart = wo.ScheduledEndDate ?? wo.ScheduledStartDate })
                .ToListAsync();

            if (relatedWorkOrders.Count == 0)
            {
                load.ReadyDate = null;
            }
            else
            {
                var maxReady = relatedWorkOrders
                    .Where(x => x.EndOrStart != null)
                    .Select(x => x.EndOrStart!.Value)
                    .DefaultIfEmpty((DateTime?)null)
                    .Max();

                load.ReadyDate = maxReady;
            }
        }

        public async Task RecalculateLoadsForPickingItemsAsync(IEnumerable<int> pickingItemIds)
        {
            var loadIds = await _db.LoadItems
                .Where(li => li.PickingListItemId != null && pickingItemIds.Contains(li.PickingListItemId!.Value))
                .Select(li => li.LoadId)
                .Distinct()
                .ToListAsync();

            foreach (var id in loadIds)
            {
                var load = await _db.Loads.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == id);
                if (load != null)
                    await RecalculateReadyDateFromWorkOrdersAsync(load);
            }

            if (loadIds.Count > 0)
                await _db.SaveChangesAsync();
        }

        private async Task<string> GenerateLoadNumber(int branchId)
        {
            var branch = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId);
            var code = branch?.Code ?? "00";
            var next = await _db.Loads.CountAsync(l => l.BranchId == branchId) + 1;
            return $"L{code}{next:000000}";
        }
    }
}
