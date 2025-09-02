using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class TruckService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public TruckService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Truck>> GetTrucksAsync(int? branchId = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            IQueryable<Truck> query = db.Trucks
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
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Trucks
                .Include(t => t.Branch)
                .Include(t => t.Driver)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task CreateAsync(Truck model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Trucks.Add(model);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Truck model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            // Load the tracked entity, copy over fields, then save.
            var existing = await db.Trucks.FirstOrDefaultAsync(t => t.Id == model.Id);
            if (existing is null) return;

            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.Identifier = model.Identifier;
            existing.CapacityWeight = model.CapacityWeight;
            existing.CapacityVolume = model.CapacityVolume;
            existing.IsActive = model.IsActive;
            existing.BranchId = model.BranchId;
            existing.DriverId = model.DriverId;

            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var entity = await db.Trucks.FindAsync(id);
            if (entity != null)
            {
                db.Trucks.Remove(entity);
                await db.SaveChangesAsync();
            }
        }
    }
}
