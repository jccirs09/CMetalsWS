using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

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
                        MachineId = item.MachineId
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
    }
}
