using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class PickingListService
    {
        private readonly ApplicationDbContext _db;

        public PickingListService(ApplicationDbContext db) => _db = db;

        public async Task<List<PickingList>> GetAsync(int? branchId = null)
        {
            var query = _db.PickingLists
                .Include(p => p.Branch)
                .Include(p => p.Truck)
                .Include(p => p.Customer)
                .Include(p => p.Items).ThenInclude(i => i.Machine)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(p => p.BranchId == branchId.Value);

            return await query.OrderByDescending(p => p.OrderDate).ToListAsync();
        }

        public async Task<PickingList?> GetByIdAsync(int id)
        {
            return await _db.PickingLists
                .Include(p => p.Customer)
                .Include(p => p.Items).ThenInclude(i => i.Machine)
                .Include(p => p.Truck)
                .Include(p => p.Branch)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateAsync(PickingList model)
        {
            model.Status = PickingListStatus.Pending;
            foreach (var item in model.Items)
                item.Status = item.Status == 0 ? PickingLineStatus.Pending : item.Status;

            if (model.CustomerId.HasValue && string.IsNullOrWhiteSpace(model.ShipToAddress))
            {
                var cust = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == model.CustomerId.Value);
                if (cust != null)
                {
                    model.CustomerName ??= cust.CustomerName;
                    model.ShipToAddress = cust.Address;
                }
            }

            _db.PickingLists.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(PickingList model)
        {
            var existing = await _db.PickingLists
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == model.Id);
            if (existing is null) return;

            existing.SalesOrderNumber = model.SalesOrderNumber;
            existing.BranchId = model.BranchId;
            existing.CustomerId = model.CustomerId;
            existing.OrderDate = model.OrderDate;
            existing.ShipDate = model.ShipDate;
            existing.CustomerName = model.CustomerName;
            existing.ShipToAddress = model.ShipToAddress;
            existing.ShippingMethod = model.ShippingMethod;
            existing.Status = model.Status;
            existing.TruckId = model.TruckId;

            var incomingIds = model.Items.Select(i => i.Id).ToHashSet();
            var toDelete = existing.Items.Where(i => !incomingIds.Contains(i.Id)).ToList();
            if (toDelete.Count > 0)
                _db.PickingListItems.RemoveRange(toDelete);

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
                    var tgt = existing.Items.First(i => i.Id == item.Id);
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
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.PickingLists.FindAsync(id);
            if (entity != null)
            {
                _db.PickingLists.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdatePickingListStatusAsync(int id)
        {
            var list = await _db.PickingLists
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

            await _db.SaveChangesAsync();
        }

        public async Task SetLineStatusAsync(int itemId, PickingLineStatus status)
        {
            var item = await _db.PickingListItems.FindAsync(itemId);
            if (item == null) return;
            item.Status = status;
            await _db.SaveChangesAsync();
            await UpdatePickingListStatusAsync(item.PickingListId);
        }
        public async Task ScheduleListAsync(int pickingListId, DateTime shipStart)
        {
            var pl = await _db.PickingLists.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == pickingListId);
            if (pl is null) return;
            pl.ShipDate = shipStart;
            await _db.SaveChangesAsync();
        }

        public async Task ScheduleLineAsync(int pickingListId, int lineId, DateTime shipStart)
        {
            var line = await _db.PickingListItems.FirstOrDefaultAsync(i => i.Id == lineId && i.PickingListId == pickingListId);
            if (line is null) return;
            line.ScheduledShipDate = shipStart;
            await _db.SaveChangesAsync();
        }

        public async Task<List<PickingListItem>> GetPendingItemsByItemIdsAsync(List<string> itemIds)
        {
            return await _db.PickingListItems
                .Include(i => i.PickingList!)
                .ThenInclude(pl => pl.Customer)
                .Where(i => i.PickingList != null && i.PickingList.Status == PickingListStatus.Pending && itemIds.Contains(i.ItemId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PickingList>> GetPendingPullingOrdersAsync(int? branchId = null)
        {
            // 1. Get IDs of all picking list items that are already part of a work order.
            var assignedPickingListItemIds = await _db.WorkOrderItems
                .Where(wi => wi.PickingListItemId != null)
                .Select(wi => wi.PickingListItemId!.Value)
                .Distinct()
                .ToListAsync();

            // 2. Find picking lists that are pending and have no items in the "assigned" list.
            var query = _db.PickingLists
                .Include(p => p.Items)
                .Where(p => p.Status == PickingListStatus.Pending && !p.Items.Any(i => assignedPickingListItemIds.Contains(i.Id)))
                .AsNoTracking();

            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            return await query.ToListAsync();
        }
    }
}
