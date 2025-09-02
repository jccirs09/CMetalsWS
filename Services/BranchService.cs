using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    /// <summary>
    /// Service for CRUD operations on Branch entities.
    /// </summary>
    public class BranchService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public BranchService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Branch>> GetBranchesAsync()
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Branches.AsNoTracking().ToListAsync();
        }

        public async Task<Branch?> GetBranchByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Branches.FindAsync(id);
        }

        public async Task AddBranchAsync(Branch branch)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Branches.Add(branch);
            await db.SaveChangesAsync();
        }

        public async Task UpdateBranchAsync(Branch branch)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Branches.Update(branch);
            await db.SaveChangesAsync();
        }

        public async Task DeleteBranchAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var entity = await db.Branches.FindAsync(id);
            if (entity != null)
            {
                db.Branches.Remove(entity);
                await db.SaveChangesAsync();
            }
        }
    }
}
