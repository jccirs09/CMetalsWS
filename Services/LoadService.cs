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
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        public LoadService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Load>> GetLoadsAsync(int? branchId = null, DateTime? onlyDate = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            IQueryable<Load> query = db.Loads
                .Include(l => l.Truck)
                .Include(l => l.Items)
                    .ThenInclude(i => i.PickingList)
                        .ThenInclude(p => p!.Customer)
                            .ThenInclude(c => c!.DestinationGroup);

            if (branchId.HasValue)
                query = query.Where(l => l.OriginBranchId == branchId.Value);

            if (onlyDate.HasValue)
            {
                var d = onlyDate.Value.Date;
                query = query.Where(l => l.ShippingDate.HasValue && l.ShippingDate.Value.Date == d);
            }

            return await query
                .OrderByDescending(l => l.ShippingDate)
                .ToListAsync();
        }

        public async Task<Load?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Loads
                .Include(l => l.Truck)
                .Include(l => l.Items)
                    .ThenInclude(i => i.PickingList)
                        .ThenInclude(p => p!.Customer)
                            .ThenInclude(c => c!.DestinationGroup)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task CreateAsync(Load load)
        {
            using var db = _dbContextFactory.CreateDbContext();
            load.LoadNumber = await GenerateLoadNumber(db, load.OriginBranchId);

            // if (load.ReadyDate == null)
            //     load.ReadyDate = await DeriveReadyDateFromPickingListsAsync(db, load);

            // await RecalculateReadyDateFromWorkOrdersAsync(db, load);
            db.Loads.Add(load);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Load load)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var existing = await db.Loads
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.Id == load.Id);
            if (existing is null) return;

            existing.Status = load.Status;
            existing.ShippingDate = load.ShippingDate;
            existing.TruckId = load.TruckId;
            existing.Notes = load.Notes;
            existing.DestinationBranchId = load.DestinationBranchId;

            existing.Items.Clear();
            foreach (var item in load.Items)
                existing.Items.Add(item);

            await db.SaveChangesAsync();
        }

        public async Task ScheduleAsync(int loadId, DateTime start, DateTime? end = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var load = await db.Loads
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.Id == loadId);
            if (load is null) return;

            var newEnd = end ?? start.AddHours(1);
            if (newEnd < start) newEnd = start;

            load.ShippingDate = start;

            if (load.Status == LoadStatus.Pending)
                load.Status = LoadStatus.Scheduled;

            await SetPickingListsScheduledForLoadAsync(db, load.Id);
            await db.SaveChangesAsync();
        }

        public static string GetLoadRegionCode(Load load)
        {
            var codes = load.Items
                .Where(i => i.PickingList?.Customer?.DestinationGroup != null)
                .Select(i => i.PickingList!.Customer!.DestinationGroup!.Name)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (codes.Count == 1) return codes[0]!;
            if (codes.Count > 1) return "MULTI";
            return "UNSET";
        }

        public async Task<DateTime?> GetPlannedReadyAsync(int loadId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var pickingListIds = await db.LoadItems
                .Where(li => li.LoadId == loadId)
                .Select(li => li.PickingListId)
                .Distinct()
                .ToListAsync();

            if (pickingListIds.Count == 0)
                return null;

            var pickingItemIds = await db.PickingListItems
                .Where(pli => pickingListIds.Contains(pli.PickingListId))
                .Select(pli => pli.Id)
                .ToListAsync();

            if (pickingItemIds.Count == 0)
                return null;

            var workTimes = await db.WorkOrders
                .Where(wo => wo.Items.Any(wi => wi.PickingListItemId != null &&
                                                pickingItemIds.Contains(wi.PickingListItemId.Value)))
                .Select(wo =>
                    wo.ScheduledEndDate != default ? (DateTime?)wo.ScheduledEndDate
                  : wo.ScheduledStartDate != default ? (DateTime?)wo.ScheduledStartDate
                  : null)
                .ToListAsync();

            var ready = workTimes
                .Where(dt => dt.HasValue)
                .Select(dt => dt!.Value)
                .DefaultIfEmpty(default)
                .Max();

            return ready == default ? null : ready;
        }

        private async Task SetPickingListsScheduledForLoadAsync(ApplicationDbContext db, int loadId)
        {
            var pickingListIds = await db.LoadItems
                .Where(li => li.LoadId == loadId)
                .Select(li => li.PickingListId)
                .Distinct()
                .ToListAsync();

            if (pickingListIds.Count == 0) return;

            var lists = await db.PickingLists
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

        // private async Task<DateTime?> DeriveReadyDateFromPickingListsAsync(ApplicationDbContext db, Load load)
        // {
        //     var pickingListIds = load.Items.Select(i => i.PickingListId).Distinct().ToList();
        //     if (pickingListIds.Count == 0) return null;

        //     var lists = await db.PickingLists
        //         .Include(p => p.Items)
        //         .Where(p => pickingListIds.Contains(p.Id))
        //         .ToListAsync();

        //     var candidates = new List<DateTime>();

        //     foreach (var pl in lists)
        //     {
        //         if (pl.ShipDate != null && pl.ShipDate.Value != default)
        //             candidates.Add(pl.ShipDate.Value);

        //         if (pl.Items != null)
        //         {
        //             candidates.AddRange(
        //                 pl.Items
        //                   .Where(i => i.ScheduledShipDate != null && i.ScheduledShipDate.Value != default)
        //                   .Select(i => i.ScheduledShipDate!.Value));
        //         }
        //     }

        //     if (candidates.Count == 0) return null;
        //     return candidates.Max();
        // }

        private async Task<string> GenerateLoadNumber(ApplicationDbContext db, int branchId)
        {
            var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId);
            var code = branch?.Code ?? "00";
            var next = await db.Loads.CountAsync(l => l.OriginBranchId == branchId) + 1;
            return $"L{code}{next:000000}";
        }
    }
}
