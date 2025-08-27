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
                .Include(p => p.Items).ThenInclude(i => i.Machine)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(p => p.BranchId == branchId.Value);

            return await query.OrderByDescending(p => p.OrderDate).ToListAsync();
        }

        public async Task<PickingList?> GetByIdAsync(int id)
        {
            return await _db.PickingLists
                .Include(p => p.Items).ThenInclude(i => i.Machine)
                .Include(p => p.Truck)
                .Include(p => p.Branch)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateAsync(PickingList model)
        {
            // Set overall status and each line status to Pending at creation.
            model.Status = PickingListStatus.Pending;
            foreach (var item in model.Items)
            {
                item.Status = PickingLineStatus.Pending;
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

        // Update a single line’s status and optionally refresh the overall list status.
        public async Task SetLineStatusAsync(int pickingListItemId, PickingLineStatus newStatus)
        {
            var item = await _db.PickingListItems
                .Include(i => i.PickingList)
                .FirstOrDefaultAsync(i => i.Id == pickingListItemId);

            if (item == null) return;

            item.Status = newStatus;
            await _db.SaveChangesAsync();

            // If all lines complete, set PickingList status to Completed
            if (item.PickingList != null)
            {
                await UpdatePickingListStatusAsync(item.PickingListId);
            }
        }

        // Recalculate PickingListStatus based on line statuses.
        public async Task UpdatePickingListStatusAsync(int pickingListId)
        {
            var pl = await _db.PickingLists
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == pickingListId);
            if (pl == null) return;

            if (pl.Items.All(i => i.Status == PickingLineStatus.Completed))
            {
                pl.Status = PickingListStatus.Completed;
            }
            else if (pl.Items.Any(i => i.Status == PickingLineStatus.InProgress))
            {
                pl.Status = PickingListStatus.InProgress;
            }
            else if (pl.Items.Any(i => i.Status == PickingLineStatus.WorkOrder))
            {
                pl.Status = PickingListStatus.Scheduled;
            }
            else if (pl.Items.Any(i => i.Status == PickingLineStatus.AssignedProduction || i.Status == PickingLineStatus.AssignedPulling))
            {
                pl.Status = PickingListStatus.Awaiting;
            }
            else
            {
                pl.Status = PickingListStatus.Pending;
            }

            await _db.SaveChangesAsync();
        }
    }
}
