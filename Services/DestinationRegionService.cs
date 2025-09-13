using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class DestinationRegionService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public DestinationRegionService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<DestinationRegion>> GetDestinationRegionsAsync()
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.DestinationRegions.ToListAsync();
        }

        public async Task<DestinationRegion?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.DestinationRegions.FindAsync(id);
        }

        public async Task CreateAsync(DestinationRegion destinationRegion)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.DestinationRegions.Add(destinationRegion);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(DestinationRegion destinationRegion)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(destinationRegion).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var destinationRegion = await db.DestinationRegions.FindAsync(id);
            if (destinationRegion != null)
            {
                db.DestinationRegions.Remove(destinationRegion);
                await db.SaveChangesAsync();
            }
        }
    }
}
