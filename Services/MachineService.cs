using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class MachineService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public MachineService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Machine>> GetMachinesAsync()
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Machines
                .Include(m => m.Branch)
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<List<Machine>> GetMachinesByBranchAsync(int branchId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Machines
                .Where(m => m.BranchId == branchId)
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Machine?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Machines.FindAsync(id);
        }

        public async Task CreateAsync(Machine model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Machines.Add(model);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Machine model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var local = db.Machines.Local.FirstOrDefault(x => x.Id == model.Id);
            if (local is not null)
                db.Entry(local).State = EntityState.Detached;

            db.Attach(model);
            db.Entry(model).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var entity = await db.Machines.FindAsync(id);
            if (entity != null)
            {
                db.Machines.Remove(entity);
                await db.SaveChangesAsync();
            }
        }
    }
}
