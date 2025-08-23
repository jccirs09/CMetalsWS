using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class MachineService
    {
        private readonly ApplicationDbContext _db;

        public MachineService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Machine>> GetMachinesAsync()
        {
            return await _db.Machines
                .Include(m => m.Branch)
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Machine?> GetByIdAsync(int id)
        {
            return await _db.Machines.FindAsync(id);
        }

        public async Task CreateAsync(Machine model)
        {
            _db.Machines.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Machine model)
        {
            _db.Machines.Update(model);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Machines.FindAsync(id);
            if (entity != null)
            {
                _db.Machines.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}
