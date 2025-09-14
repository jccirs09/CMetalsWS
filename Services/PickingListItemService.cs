using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class PickingListItemService
    {
        private readonly ApplicationDbContext _db;
        private readonly PickingListService _pickingListService;

        public PickingListItemService(ApplicationDbContext db, PickingListService pickingListService)
        {
            _db = db;
            _pickingListService = pickingListService;
        }

        public Task<PickingListItem?> GetAsync(int id) =>
            _db.Set<PickingListItem>()
               .Include(x => x.Machine)
               .FirstOrDefaultAsync(x => x.Id == id);

        public async Task SaveAsync(PickingListItem model)
        {
            if (model.Id == 0)
            {
                model.Status = PickingLineStatus.Pending;
                _db.Add(model);
            }
            else
            {
                _db.Update(model);
            }
            await _db.SaveChangesAsync();
        }

        public async Task AssignToProductionAsync(int pickingListItemId, int machineId)
        {
            var item = await _db.Set<PickingListItem>().FindAsync(pickingListItemId)
                       ?? throw new InvalidOperationException("Line not found.");
            item.Status = PickingLineStatus.AssignedProduction;
            item.MachineId = machineId;
            await _db.SaveChangesAsync();

            // Recalculate overall PickingListStatus to Awaiting
            if (item.PickingListId != 0)
                await _pickingListService.UpdatePickingListStatusAsync(item.PickingListId, PickingListStatus.Awaiting);
        }

        public async Task AssignToPullingAsync(int pickingListItemId)
        {
            var item = await _db.Set<PickingListItem>().FindAsync(pickingListItemId)
                       ?? throw new InvalidOperationException("Line not found.");
            item.Status = PickingLineStatus.AssignedPulling;
            item.MachineId = null;
            await _db.SaveChangesAsync();

            if (item.PickingListId != 0)
                await _pickingListService.UpdatePickingListStatusAsync(item.PickingListId, PickingListStatus.Awaiting);
        }
    }
}
