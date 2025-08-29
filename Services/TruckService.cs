using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class TruckService
    {
        private readonly ApplicationDbContext _db;

        public TruckService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Truck>> GetTrucksAsync(int? branchId = null)
        {
            IQueryable<Truck> query = _db.Trucks
                .Include(t => t.Branch)
                .Include(t => t.Driver)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(t => t.BranchId == branchId.Value);

            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        // Use AsNoTracking when fetching for edit to avoid tracking conflicts in the UI
        public async Task<Truck?> GetByIdAsync(int id)
        {
            return await _db.Trucks
                .Include(t => t.Branch)
                .Include(t => t.Driver)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task CreateAsync(Truck model)
        {
            _db.Trucks.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Truck model)
        {
            // Load the tracked entity, copy over fields, then save.
            var existing = await _db.Trucks.FirstOrDefaultAsync(t => t.Id == model.Id);
            if (existing is null) return;

            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.Identifier = model.Identifier;
            existing.CapacityWeight = model.CapacityWeight;
            existing.CapacityVolume = model.CapacityVolume;
            existing.IsActive = model.IsActive;
            existing.BranchId = model.BranchId;
            existing.DriverId = model.DriverId;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Trucks.FindAsync(id);
            if (entity != null)
            {
                _db.Trucks.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}
