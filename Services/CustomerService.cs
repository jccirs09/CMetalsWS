using CMetalsWS.Data;
using CMetalsWS.Models;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class CustomerService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ICustomerEnrichmentService _customerEnrichmentService;

        public CustomerService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ICustomerEnrichmentService customerEnrichmentService)
        {
            _dbContextFactory = dbContextFactory;
            _customerEnrichmentService = customerEnrichmentService;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Customer>> SearchAsync(string term)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (string.IsNullOrWhiteSpace(term)) return new List<Customer>();

            var t = term.ToLower();
            return await db.Customers.AsNoTracking()
                .Where(c => c.Active && (c.CustomerCode.ToLower().Contains(t) || c.CustomerName.ToLower().Contains(t)))
                .OrderBy(c => c.CustomerCode)
                .Take(20)
                .ToListAsync();
        }

        public async Task<GetCustomersResult> GetCustomersAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string? regionFilter,
            string? groupFilter,
            bool? activeFilter,
            string? sortBy,
            bool descending)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var query = db.Customers.AsNoTracking();

            // Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var t = searchTerm.ToLower();
                query = query.Where(c =>
                    c.CustomerCode.ToLower().Contains(t) ||
                    c.CustomerName.ToLower().Contains(t) ||
                    (c.City != null && c.City.ToLower().Contains(t)) ||
                    (c.FullAddress != null && c.FullAddress.ToLower().Contains(t)));
            }

            if (!string.IsNullOrWhiteSpace(regionFilter))
            {
                if (Enum.TryParse<DestinationRegionCategory>(regionFilter, true, out var region))
                {
                    query = query.Where(c => c.DestinationRegionCategory == region);
                }
            }

            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                query = query.Where(c => c.DestinationGroupCategory != null && c.DestinationGroupCategory.ToLower().Contains(groupFilter.ToLower()));
            }

            if (activeFilter.HasValue)
            {
                query = query.Where(c => c.Active == activeFilter.Value);
            }

            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // This is a simplified sorting implementation. A real-world app might use a more robust solution.
                var prop = typeof(Customer).GetProperty(sortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    query = descending
                        ? query.OrderByDescending(c => EF.Property<object>(c, prop.Name))
                        : query.OrderBy(c => EF.Property<object>(c, prop.Name));
                }
            }
            else
            {
                query = query.OrderBy(c => c.CustomerCode);
            }

            // Paging
            var customers = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new GetCustomersResult { Customers = customers, TotalCount = totalCount };
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            using var db = _dbContextFactory.CreateDbContext();
            customer.ModifiedUtc = DateTime.UtcNow;
            db.Customers.Update(customer);
            await db.SaveChangesAsync();
        }

        public async Task CreateCustomerAsync(Customer customer)
        {
            using var db = _dbContextFactory.CreateDbContext();
            customer.CreatedUtc = DateTime.UtcNow;
            db.Customers.Add(customer);
            await db.SaveChangesAsync();
        }

        public async Task<Customer?> RecomputeCustomerRegionAsync(int customerId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var customer = await db.Customers.FindAsync(customerId);
            if (customer == null || string.IsNullOrWhiteSpace(customer.FullAddress))
            {
                return null;
            }

            customer = await _customerEnrichmentService.EnrichAndCategorizeCustomerAsync(customer, customer.FullAddress);
            await UpdateCustomerAsync(customer);
            return customer;
        }

        public async Task<Customer?> GeocodeCustomerAsync(int customerId, string address)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var customer = await db.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return null;
            }

            customer = await _customerEnrichmentService.EnrichAndCategorizeCustomerAsync(customer, address);
            await UpdateCustomerAsync(customer);
            return customer;
        }

        public async Task<CustomerImportReport> ImportCustomersAsync(Stream stream)
        {
            var report = new CustomerImportReport();
            var rows = stream.Query<CustomerImportDto>().ToList();
            report.TotalRows = rows.Count;

            using var db = _dbContextFactory.CreateDbContext();

            foreach (var row in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(row.CustomerCode))
                    {
                        report.Errors.Add($"Skipping row with empty CustomerCode.");
                        continue;
                    }

                    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerCode == row.CustomerCode);
                    if (customer == null)
                    {
                        customer = new Customer { CustomerCode = row.CustomerCode, CreatedUtc = DateTime.UtcNow };
                        db.Customers.Add(customer);
                    }

                    // Map DTO to entity
                    customer.CustomerName = row.CustomerName;
                    customer.BusinessHours = row.BusinessHours;
                    customer.ContactNumber = row.ContactNumber;
                    // ... map other properties from DTO ...

                    // Enrich and categorize
                    var address = row.Address; // Assuming a single address column in Excel
                    customer = await _customerEnrichmentService.EnrichAndCategorizeCustomerAsync(customer, address);

                    customer.ModifiedUtc = DateTime.UtcNow;

                    await db.SaveChangesAsync();
                    report.SuccessfulImports++;
                }
                catch (Exception ex)
                {
                    report.FailedImports++;
                    report.Errors.Add($"Failed to import customer {row.CustomerCode}: {ex.Message}");
                }
            }

            return report;
        }
    }

    // DTO for Excel import
    public class CustomerImportDto
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BusinessHours { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        // Add other fields from Excel as needed
    }
}
