using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class PickingListService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public PickingListService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        public async Task<List<PickingList>> GetAsync(int? branchId = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var query = db.PickingLists
                .Include(p => p.Branch)
                .Include(p => p.Customer)
                .Include(p => p.Items).ThenInclude(i => i.Machine)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(p => p.BranchId == branchId.Value);

            return await query.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<List<PickingList>> GetAvailableForLoadingAsync(int? branchId = null)
        {
            using var db = _dbContextFactory.CreateDbContext();

            var loadedItemIds = await db.LoadItems
                .Select(li => li.PickingListItemId)
                .Distinct()
                .ToListAsync();

            var query = db.PickingLists
                .Include(p => p.Customer)
                .Include(p => p.Items)
                .Include(p => p.Branch)
                .Where(p => p.Status == PickingListStatus.Completed &&
                             p.Items.Any(i => !loadedItemIds.Contains(i.Id)))
                .AsNoTracking();

            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            return await query.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<PickingList?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.PickingLists
                .Include(p => p.Customer)
                .Include(p => p.Items).ThenInclude(i => i.Machine)
                .Include(p => p.Branch)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateAsync(PickingList model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            model.Status = PickingListStatus.Pending;
            foreach (var item in model.Items)
                item.Status = item.Status == 0 ? PickingLineStatus.Pending : item.Status;

            db.PickingLists.Add(model);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(PickingList model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var existing = await db.PickingLists
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == model.Id);
            if (existing is null) return;

            existing.SalesOrderNumber = model.SalesOrderNumber;
            existing.BranchId = model.BranchId;
            existing.CustomerId = model.CustomerId;
            existing.Status = model.Status;

            var incomingIds = model.Items.Select(i => i.Id).ToHashSet();
            var toDelete = existing.Items.Where(i => !incomingIds.Contains(i.Id)).ToList();
            if (toDelete.Count > 0)
                db.PickingListItems.RemoveRange(toDelete);
            var existingItems = existing.Items.ToDictionary(i => i.Id);

            foreach (var item in model.Items)
            {
                if (item.Id == 0)
                {
                    existing.Items.Add(new PickingListItem
                    {
                        LineNumber = item.LineNumber,
                        ItemId = item.ItemId,
                        ItemDescription = item.ItemDescription,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        Width = item.Width,
                        Length = item.Length,
                        Weight = item.Weight,
                        MachineId = item.MachineId,
                        Status = item.Status == 0 ? PickingLineStatus.Pending : item.Status
                    });
                }
                else
                {
                    if (existingItems.TryGetValue(item.Id, out var tgt))
                    {
                        tgt.LineNumber = item.LineNumber;
                        tgt.ItemId = item.ItemId;
                        tgt.ItemDescription = item.ItemDescription;
                        tgt.Quantity = item.Quantity;
                        tgt.Unit = item.Unit;
                        tgt.Width = item.Width;
                        tgt.Length = item.Length;
                        tgt.Weight = item.Weight;
                        tgt.MachineId = item.MachineId;
                        tgt.Status = item.Status;
                        tgt.PulledQuantity = item.PulledQuantity;
                        tgt.PulledWeight = item.PulledWeight;
                    }
                }
            }

            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var entity = await db.PickingLists.FindAsync(id);
            if (entity != null)
            {
                db.PickingLists.Remove(entity);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdatePickingListStatusAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var list = await db.PickingLists
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (list is null) return;

            if (list.Items.All(i => i.Status == PickingLineStatus.AssignedProduction))
                list.Status = PickingListStatus.Awaiting;
            else if (list.Items.All(i => i.Status == PickingLineStatus.AssignedPulling))
                list.Status = PickingListStatus.Awaiting;
            else if (list.Items.All(i => i.Status == PickingLineStatus.Completed))
                list.Status = PickingListStatus.Completed;
            else if (list.Items.Any(i => i.Status == PickingLineStatus.InProgress))
                list.Status = PickingListStatus.InProgress;
            else
                list.Status = PickingListStatus.Pending;

            await db.SaveChangesAsync();
        }

        public async Task SetLineStatusAsync(int itemId, PickingLineStatus status)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var item = await db.PickingListItems.FindAsync(itemId);
            if (item == null) return;
            item.Status = status;
            await db.SaveChangesAsync();
            await UpdatePickingListStatusAsync(item.PickingListId);
        }
        //TODO: Refactor this method to work with the new data model
        //public async Task ScheduleListAsync(int pickingListId, DateTime shipStart)
        //{
        //    using var db = _dbContextFactory.CreateDbContext();
        //    var pl = await db.PickingLists.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == pickingListId);
        //    if (pl is null) return;
        //    //pl.ShipDate = shipStart;
        //    await db.SaveChangesAsync();
        //}

        public async Task ScheduleLineAsync(int pickingListId, int lineId, DateTime shipStart)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var line = await db.PickingListItems.FirstOrDefaultAsync(i => i.Id == lineId && i.PickingListId == pickingListId);
            if (line is null) return;
            line.ScheduledShipDate = shipStart;
            await db.SaveChangesAsync();
        }

        public async Task<List<PickingListItem>> GetPendingItemsByItemIdsAsync(List<string> itemIds)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.PickingListItems
                .Include(i => i.PickingList!)
                .ThenInclude(pl => pl.Customer)
                .Where(i => i.PickingList != null && (i.Status == PickingLineStatus.Pending || i.Status == PickingLineStatus.Awaiting) && itemIds.Contains(i.ItemId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PickingListItem>> GetPendingItemsByParentItemIdAsync(string parentItemId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.PickingListItems
                .Include(i => i.PickingList!)
                .ThenInclude(pl => pl.Customer)
                .Where(i => i.PickingList != null && (i.Status == PickingLineStatus.Pending || i.Status == PickingLineStatus.Awaiting) && i.ItemId == parentItemId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PickingList>> GetPendingPullingOrdersAsync(int? branchId = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var query = db.PickingLists
                .Include(p => p.Items)
                .Where(p => p.Status == PickingListStatus.Pending &&
                            !p.Items.Any(i => db.WorkOrderItems.Any(wi => wi.PickingListItemId == i.Id)))
                .AsNoTracking();

            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<PickingListItem>> GetPullingTasksAsync(int? branchId = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var query = db.PickingListItems
                .Include(i => i.PickingList)
                .Where(i => i.PickingList != null && i.Status == PickingLineStatus.AssignedPulling)
                .AsNoTracking();

            if (branchId.HasValue)
            {
                query = query.Where(i => i.PickingList!.BranchId == branchId.Value);
            }

            return await query.OrderBy(i => i.PickingList!.Id).ToListAsync();
        }

        public async Task UpdatePulledQuantitiesAsync(int itemId, decimal? pulledQuantity, decimal pulledWeight)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var item = await db.PickingListItems.FindAsync(itemId);
            if (item == null) return;

            item.PulledQuantity = pulledQuantity;
            item.PulledWeight = pulledWeight;

            await db.SaveChangesAsync();
        }
    }

}
