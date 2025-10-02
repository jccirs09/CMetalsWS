using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
            return await db.DestinationRegions.Include(dr => dr.Branches).ToListAsync();
        }

        public async Task<DestinationRegion?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.DestinationRegions.Include(dr => dr.Branches).FirstOrDefaultAsync(dr => dr.Id == id);
        }

        public async Task CreateAsync(DestinationRegion destinationRegion, IEnumerable<int> branchIds)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var branches = await db.Branches.Where(b => branchIds.Contains(b.Id)).ToListAsync();
            destinationRegion.Branches = branches;
            db.DestinationRegions.Add(destinationRegion);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(DestinationRegion destinationRegion, IEnumerable<int> branchIds)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var regionToUpdate = await db.DestinationRegions.Include(dr => dr.Branches).FirstOrDefaultAsync(dr => dr.Id == destinationRegion.Id);
            if (regionToUpdate != null)
            {
                regionToUpdate.Name = destinationRegion.Name;
                regionToUpdate.Type = destinationRegion.Type;
                var branches = await db.Branches.Where(b => branchIds.Contains(b.Id)).ToListAsync();
                regionToUpdate.Branches = branches;
                await db.SaveChangesAsync();
            }
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

        public DestinationRegionCategory GetDestinationRegionCategory(DestinationRegion region)
        {
            return region.Type.ToUpper().Trim() switch
            {
                "LOCAL" => DestinationRegionCategory.LOCAL,
                "ISLAND" => DestinationRegionCategory.ISLAND,
                "OKANAGAN" => DestinationRegionCategory.OKANAGAN,
                _ => DestinationRegionCategory.OUT_OF_TOWN,
            };
        }

    }
}
