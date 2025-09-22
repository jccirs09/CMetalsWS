using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class ShiftService
    {
        private readonly ApplicationDbContext _context;

        public ShiftService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Shift>> GetShiftsAsync(int branchId)
        {
            return await _context.Shifts
                .Where(s => s.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<Shift?> GetShiftAsync(int id)
        {
            return await _context.Shifts.FindAsync(id);
        }

        public async Task AddShiftAsync(Shift shift)
        {
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateShiftAsync(Shift shift)
        {
            _context.Entry(shift).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteShiftAsync(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift != null)
            {
                _context.Shifts.Remove(shift);
                await _context.SaveChangesAsync();
            }
        }
    }
}
