// Services/BranchService.cs
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    /// <summary>
    /// Service for CRUD operations on Branch entities.
    /// </summary>
    public class BranchService
    {
        private readonly ApplicationDbContext _context;

        public BranchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Branch>> GetBranchesAsync()
        {
            return await _context.Branches.AsNoTracking().ToListAsync();
        }

        public async Task<Branch?> GetBranchByIdAsync(int id)
        {
            return await _context.Branches.FindAsync(id);
        }

        public async Task AddBranchAsync(Branch branch)
        {
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBranchAsync(Branch branch)
        {
            _context.Branches.Update(branch);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBranchAsync(int id)
        {
            var entity = await _context.Branches.FindAsync(id);
            if (entity != null)
            {
                _context.Branches.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
