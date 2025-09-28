using CMetalsWS.Data;
using CMetalsWS.Models;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            return await db.Customers
                .AsNoTracking()
                .Include(c => c.DestinationGroup)
                .Include(c => c.DestinationRegion)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Customer>> SearchAsync(string term)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (string.IsNullOrWhiteSpace(term)) return new List<Customer>();

            var t = term.ToLower();
            return await db.Customers.AsNoTracking()
                .Where(c => c.Active && (c.CustomerCode.ToLower().Contains(t) || c.CustomerName.ToLower().Contains(t)))
                .OrderBy(c => c.CustomerName)
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
            IQueryable<Customer> query = db.Customers.AsNoTracking()
                .Include(c => c.DestinationRegion)
                .Include(c => c.DestinationGroup);

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
                query = query.Where(c => c.DestinationRegion != null && c.DestinationRegion.Name.ToLower().Contains(regionFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                query = query.Where(c => c.DestinationGroup != null && c.DestinationGroup.Name.ToLower().Contains(groupFilter.ToLower()));
            }

            if (activeFilter.HasValue)
            {
                query = query.Where(c => c.Active == activeFilter.Value);
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
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

            var customers = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new GetCustomersResult { Customers = customers, TotalCount = totalCount };
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            using var db = _dbContextFactory.CreateDbContext();
            ParseFullAddress(customer);
            customer.ModifiedUtc = DateTime.UtcNow;
            db.Customers.Update(customer);
            await db.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var customer = await db.Customers.FindAsync(customerId);
            if (customer != null)
            {
                db.Customers.Remove(customer);
                await db.SaveChangesAsync();
            }
        }

        public async Task CreateCustomerAsync(Customer customer)
        {
            using var db = _dbContextFactory.CreateDbContext();
            ParseFullAddress(customer);
            customer.CreatedUtc = DateTime.UtcNow;
            db.Customers.Add(customer);
            await db.SaveChangesAsync();
        }


        public async Task<List<CustomerImportRow>> PreviewImportAsync(Stream stream)
        {
            var rows = stream.Query<CustomerImportDto>().ToList();
            var tasks = new List<Task<CustomerImportRow>>();

            foreach (var row in rows)
            {
                tasks.Add(ProcessImportRow(row));
            }

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        private async Task<CustomerImportRow> ProcessImportRow(CustomerImportDto row)
        {
            var importRow = new CustomerImportRow { Dto = row };
            // Since we removed the enrichment service, we no longer have candidates.
            // We will just return the row with the DTO.
            // The user will have to manually edit the customer later if the address is not correct.
            return await Task.FromResult(importRow);
        }

        public async Task<CustomerImportReport> CommitImportAsync(List<CustomerImportRow> importRows)
        {
            var report = new CustomerImportReport { TotalRows = importRows.Count };
            var _random = new Random();
            foreach (var row in importRows)
            {
                try
                {
                    using var db = _dbContextFactory.CreateDbContext();

                    if (!string.IsNullOrWhiteSpace(row.Error))
                    {
                        report.FailedImports++;
                        report.Errors.Add($"Row for {row.Dto.CustomerCode} had a processing error: {row.Error}");
                        continue;
                    }

                    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerCode == row.Dto.CustomerCode);
                    var isNew = customer == null;
                    if (isNew)
                    {
                        customer = new Customer { CustomerCode = row.Dto.CustomerCode, CreatedUtc = DateTime.UtcNow };

                        // Add random max skid weight capacity between 1500 to 6000 lbs in increments of 500 lbs
                        customer.MaxSkidCapacity = _random.Next(3, 13) * 500; // 1500 to 6000

                        // Add random max slit coil weight from 1000 to 4000 lbs in increments of 500 lbs
                        customer.MaxSlitCoilWeight = _random.Next(2, 9) * 500; // 1000 to 4000

                        db.Customers.Add(customer);
                    }

                    // Always update basic info from the spreadsheet
                    customer!.CustomerName = row.Dto.CustomerName;
                    customer.BusinessHours = row.Dto.BusinessHours;
                    customer.ContactNumber = row.Dto.ContactNumber;

                    // Update address info from the spreadsheet
                    customer.FullAddress = row.Dto.Address;
                    ParseFullAddress(customer);

                    customer.ModifiedUtc = DateTime.UtcNow;
                    await db.SaveChangesAsync(); // Commit each record individually for robustness
                    report.SuccessfulImports++;
                }
                catch (Exception ex)
                {
                    report.FailedImports++;
                    report.Errors.Add($"Failed to import customer {row.Dto.CustomerCode}: {ex.Message}");
                }
            }

            return report;
        }
        private void ParseFullAddress(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.FullAddress)) return;

            var addressParts = customer.FullAddress.Split(',').Select(p => p.Trim()).ToList();

            // Reset fields
            customer.Street1 = null;
            customer.Street2 = null;
            customer.City = null;
            customer.Province = null;
            customer.PostalCode = null;
            customer.Country = null;

            if (addressParts.Count > 0)
            {
                customer.Street1 = addressParts[0];
                addressParts.RemoveAt(0);
            }

            if (addressParts.Count > 0)
            {
                customer.City = addressParts[0];
                addressParts.RemoveAt(0);
            }

            foreach(var part in addressParts)
            {
                // Canadian postal code
                var postalCodeRegex = new Regex(@"\b[A-Z]\d[A-Z] ?\d[A-Z]\d\b", RegexOptions.IgnoreCase);
                var match = postalCodeRegex.Match(part);
                if (match.Success)
                {
                    customer.PostalCode = match.Value;
                    var province = part.Replace(match.Value, "").Trim();
                    if(!string.IsNullOrWhiteSpace(province))
                    {
                        customer.Province = province;
                    }
                    continue;
                }

                if (part.Equals("Canada", StringComparison.OrdinalIgnoreCase) || part.Equals("USA", StringComparison.OrdinalIgnoreCase))
                {
                    customer.Country = part;
                    continue;
                }

                // If not postal code or country, it must be province.
                if(string.IsNullOrWhiteSpace(customer.Province))
                {
                    customer.Province = part;
                }
            }
        }
    }

}
