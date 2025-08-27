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
                        .ThenInclude(p => p.Customer); // <- Customer now wired

            if (branchId.HasValue)
                query = query.Where(l => l.BranchId == branchId.Value);

            if (onlyDate.HasValue)
            {
                var d = onlyDate.Value.Date;
                query = query.Where(l =>
                    (l.ScheduledStart.HasValue && l.ScheduledStart.Value.Date == d) ||
                    (l.ScheduledDate.HasValue && l.ScheduledDate.Value.Date == d));
            }

            return await query
                .OrderByDescending(l => l.ScheduledStart)
                .ToListAsync();
        }

        public async Task<Load?> GetByIdAsync(int id)
        {
            return await _db.Loads
                .Include(l => l.Truck)
                .Include(l => l.Items)
                    .ThenInclude(i => i.PickingList)
                        .ThenInclude(p => p.Customer)
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

            // Replace items (simple sync)
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

            // Mark related picking lists as Scheduled (since they’re now on a load schedule)
            await SetPickingListsScheduledForLoadAsync(load.Id);

            await RecalculateReadyDateFromWorkOrdersAsync(load);
            await _db.SaveChangesAsync();
        }

        /// Computes a grouping key for routing from the customers on this load’s picking list items.
        /// If multiple customer regions are present, returns "MULTI"; if none, "UNSET".
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

        /// Returns a planned "ready" timestamp for an unscheduled load:
        /// the time all related work orders are done (max of each WO EndOrStart across included items).
        public async Task<DateTime?> GetPlannedReadyAsync(int loadId)
        {
            // Gather all PickingListItem ids attached to this load
            var pickingItemIds = await _db.LoadItems
                .Where(li => li.LoadId == loadId && li.PickingListItemId != null)
                .Select(li => li.PickingListItemId!.Value)
                .ToListAsync();

            if (pickingItemIds.Count == 0)
                return null;

            // Work orders that cover any of those items
            var workTimes = await _db.WorkOrders
                .Where(wo => wo.Items.Any(wi => wi.PickingListItemId != null &&
                                                pickingItemIds.Contains(wi.PickingListItemId.Value)))
                .Select(wo => wo.ScheduledEndDate ?? wo.ScheduledStartDate)
                .ToListAsync();

            if (workTimes.Count == 0) return null;

            // A load is "ready" when all its items are ready -> use the latest time
            var ready = workTimes
                .Where(dt => dt.HasValue)
                .Select(dt => dt!.Value)
                .DefaultIfEmpty()
                .Max();

            return ready == default ? null : ready;
        }

        /// Recomputes and stores Load.ReadyDate based on related work orders for the items on the load.
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
                // If your Load model has ReadyDate, keep it in sync; otherwise remove these lines.
                load.ReadyDate = null;
                return;
            }

            var relatedWorkOrders = await _db.WorkOrders
                .Where(wo => wo.Items.Any(wi => wi.PickingListItemId != null &&
                                                pickingItemIds.Contains(wi.PickingListItemId.Value)))
                .Select(wo => wo.ScheduledEndDate ?? wo.ScheduledStartDate)
                .ToListAsync();

            if (relatedWorkOrders.Count == 0)
            {
                load.ReadyDate = null;
            }
            else
            {
                var maxReady = relatedWorkOrders
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .DefaultIfEmpty()
                    .Max();

                load.ReadyDate = maxReady == default ? null : maxReady;
            }
        }

        /// When picking list item statuses change (e.g., after WO completion), recalc any loads that include them.
        public async Task RecalculateLoadsForPickingItemsAsync(IEnumerable<int> pickingItemIds)
        {
            var ids = pickingItemIds?.ToList() ?? new List<int>();
            if (ids.Count == 0) return;

            var loadIds = await _db.LoadItems
                .Where(li => li.PickingListItemId != null && ids.Contains(li.PickingListItemId!.Value))
                .Select(li => li.LoadId)
                .Distinct()
                .ToListAsync();

            foreach (var id in loadIds)
            {
                var load = await _db.Loads
                    .Include(l => l.Items)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (load != null)
                    await RecalculateReadyDateFromWorkOrdersAsync(load);
            }

            if (loadIds.Count > 0)
                await _db.SaveChangesAsync();
        }

        private async Task SetPickingListsScheduledForLoadAsync(int loadId)
        {
            // Find all picking lists attached (via load items -> picking list items -> picking list)
            var pickingListIds = await _db.LoadItems
                .Where(li => li.LoadId == loadId && li.PickingListId != null)
                .Join(_db.PickingListItems,
                    li => li.PickingListId,
                    pli => pli.Id,
                    (li, pli) => pli.PickingListId)
                .Distinct()
                .ToListAsync();

            if (pickingListIds.Count == 0) return;

            var lists = await _db.PickingLists
                .Where(p => pickingListIds.Contains(p.Id))
                .ToListAsync();

            foreach (var pl in lists)
            {                
                if (pl.Status == PickingListStatus.Pending ||
                    pl.Status == PickingListStatus.Awaiting ||
                    pl.Status == PickingListStatus.InProgress ||
                    pl.Status == PickingListStatus.Picked ||
                    pl.Status == PickingListStatus.ReadyToShip)
                {
                    pl.Status = PickingListStatus.Scheduled;
                }
            }
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
