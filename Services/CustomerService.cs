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
            // Read from the beginning just in case
            if (stream.CanSeek) stream.Position = 0;

            var raw = stream.Query<CustomerImportDto>().ToList();

            // Clean and filter
            var rows = raw
                .Where(r => !string.IsNullOrWhiteSpace(r.CustomerCode) || !string.IsNullOrWhiteSpace(r.CustomerName))
                .Select(r => new CustomerImportDto
                {
                    CustomerCode  = r.CustomerCode?.Trim(),
                    CustomerName  = r.CustomerName?.Trim(),
                    FullAddress   = CleanAddress(r.FullAddress),
                    BusinessHours = r.BusinessHours?.Trim(),
                    ContactNumber = r.ContactNumber?.Trim(),
                    Latitude = r.Latitude,
                    Longitude = r.Longitude
                })
                .ToList();

            var tasks = rows.Select(r => ProcessImportRow(r)).ToList();
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
                    customer.Latitude = row.Dto.Latitude;
                    customer.Longitude = row.Dto.Longitude;

                    // Update address info from the spreadsheet
                    customer.FullAddress = row.Dto.FullAddress; // Already cleaned in PreviewImportAsync
                    ParseFullAddress(customer);

                    // If still null/empty, add a non-fatal info line so you know which rows had no address
                    if (string.IsNullOrWhiteSpace(customer.FullAddress))
                    {
                        report.Errors.Add($"INFO: No FullAddress for {row.Dto.CustomerCode} - {row.Dto.CustomerName}. Skipped parsing.");
                    }

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

        private static string? CleanAddress(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            // collapse whitespace & replace line breaks with comma + space
            var normalized = Regex.Replace(s, @"\r\n|\n|\r", ", ");
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            return normalized.TrimEnd(','); // common typo in exports
        }
        private void ParseFullAddress(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.FullAddress)) return;

            // Reset fields
            customer.Street1 = null;
            customer.Street2 = null;
            customer.City = null;
            customer.Province = null;
            customer.PostalCode = null;
            customer.Country = null;

            var address = customer.FullAddress;

            // 1. Extract Country from the end of the string
            var countryRegex = new Regex(@"(,?\s*(?<country>USA|Canada))$", RegexOptions.IgnoreCase);
            var countryMatch = countryRegex.Match(address);
            if (countryMatch.Success)
            {
                customer.Country = countryMatch.Groups["country"].Value.ToUpper();
                address = address.Substring(0, countryMatch.Index).Trim();
            }

            // 2. Extract Postal Code from the end of the remaining string
            var postalCodeRegex = new Regex(@"(,?\s*(?<postal_code>(?<zip>\d{5}(?:-\d{4})?)|(?<postal>[A-Z]\d[A-Z]\s?\d[A-Z]\d)))$", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
            var postalMatch = postalCodeRegex.Match(address);
            if (postalMatch.Success)
            {
                customer.PostalCode = postalMatch.Groups["postal_code"].Value;
                address = address.Substring(0, postalMatch.Index).Trim();
            }

            // 3. Split the rest by comma
            var addressParts = address.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(p => p.Trim())
                                      .Where(p => !string.IsNullOrEmpty(p))
                                      .ToList();

            if (addressParts.Count == 0) return;

            // 4. Province/State is likely the last part.
            customer.Province = addressParts.Last();
            addressParts.RemoveAt(addressParts.Count - 1);

            if (addressParts.Count == 0) return;

            // 5. City is now the last part.
            customer.City = addressParts.Last();
            addressParts.RemoveAt(addressParts.Count - 1);

            if (addressParts.Count > 0)
            {
                // 6. The rest is street address.
                customer.Street1 = addressParts[0];
                if (addressParts.Count > 1)
                {
                    customer.Street2 = string.Join(", ", addressParts.Skip(1));
                }
            }

        }
    }

}
