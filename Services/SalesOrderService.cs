using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class SalesOrderService
    {
        private readonly ApplicationDbContext _db;

        public SalesOrderService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<SalesOrder>> GetAsync(int? branchId = null)
        {
            IQueryable<SalesOrder> query = _db.SalesOrders
                .Include(o => o.Items);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            return await query.OrderByDescending(o => o.CreatedDate).ToListAsync();
        }

        public async Task<SalesOrder?> GetByIdAsync(int id)
        {
            return await _db.SalesOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task CreateAsync(SalesOrder order)
        {
            order.OrderNumber = await GenerateOrderNumber(order.BranchId);
            _db.SalesOrders.Add(order);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(SalesOrder order)
        {
            var existing = await _db.SalesOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == order.Id);
            if (existing is null) return;

            existing.Status = order.Status;
            existing.Items.Clear();
            foreach (var item in order.Items)
            {
                existing.Items.Add(item);
            }
            await _db.SaveChangesAsync();
        }

        private async Task<string> GenerateOrderNumber(int branchId)
        {
            var branchCode = (await _db.Branches.FindAsync(branchId))?.Code ?? "00";
            var count = await _db.SalesOrders.CountAsync(o => o.BranchId == branchId) + 1;
            return $"SO{branchCode}{count:000000}";
        }
    }
}
