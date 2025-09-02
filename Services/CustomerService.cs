using CMetalsWS.Data;
using CMetalsWS.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class CustomerService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        public CustomerService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetByCodeAsync(string customerCode)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (string.IsNullOrWhiteSpace(customerCode)) return null;
            var code = customerCode.Trim();
            return await db.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerCode == code);
        }

        public async Task<List<Customer>> SearchAsync(string term, int take = 20)
        {
            using var db = _dbContextFactory.CreateDbContext();
            term = term?.Trim() ?? string.Empty;
            IQueryable<Customer> q = db.Customers.AsNoTracking().Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(term))
            {
                var t = term.ToLower();
                q = q.Where(c =>
                    c.CustomerCode.ToLower().Contains(t) ||
                    c.CustomerName.ToLower().Contains(t) ||
                    (c.LocationCode != null && c.LocationCode.ToLower().Contains(t)));
            }

            return await q
                .OrderBy(c => c.CustomerCode)
                .ThenBy(c => c.CustomerName)
                .Take(Math.Clamp(take, 1, 100))
                .ToListAsync();
        }
    }
}
