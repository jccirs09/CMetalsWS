using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class DestinationGroupService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public DestinationGroupService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<DestinationGroup>> GetDestinationGroupsAsync()
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.DestinationGroups.ToListAsync();
        }

        public async Task<DestinationGroup?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.DestinationGroups.FindAsync(id);
        }

        public async Task CreateAsync(DestinationGroup destinationGroup)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.DestinationGroups.Add(destinationGroup);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(DestinationGroup destinationGroup)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(destinationGroup).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var destinationGroup = await db.DestinationGroups.FindAsync(id);
            if (destinationGroup != null)
            {
                db.DestinationGroups.Remove(destinationGroup);
                await db.SaveChangesAsync();
            }
        }
    }
}
