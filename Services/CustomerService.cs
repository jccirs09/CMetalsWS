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
        private readonly IGooglePlacesService _googlePlacesService;

        public CustomerService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ICustomerEnrichmentService customerEnrichmentService, IGooglePlacesService googlePlacesService)
        {
            _dbContextFactory = dbContextFactory;
            _customerEnrichmentService = customerEnrichmentService;
            _googlePlacesService = googlePlacesService;
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
            var query = db.Customers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var t = searchTerm.ToLower();
                query = query.Where(c =>
                    c.CustomerCode.ToLower().Contains(t) ||
                    c.CustomerName.ToLower().Contains(t) ||
                    (c.City != null && c.City.ToLower().Contains(t)) ||
                    (c.Address != null && c.Address.ToLower().Contains(t)));
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

        public async Task<CustomerEnrichmentResult> RecomputeCustomerRegionAsync(int customerId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var customer = await db.Customers.FindAsync(customerId);
            if (customer == null || string.IsNullOrWhiteSpace(customer.Address))
            {
                return new CustomerEnrichmentResult { EnrichedCustomer = customer };
            }

            var result = await _customerEnrichmentService.EnrichAndCategorizeCustomerAsync(customer, customer.Address);
            if (result.EnrichedCustomer != null)
            {
                await UpdateCustomerAsync(result.EnrichedCustomer);
            }
            return result;
        }

        public async Task<CustomerEnrichmentResult> GeocodeCustomerAsync(int customerId, string address)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var customer = await db.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return new CustomerEnrichmentResult();
            }

            var result = await _customerEnrichmentService.EnrichAndCategorizeCustomerAsync(customer, address);
            if (result.EnrichedCustomer != null)
            {
                await UpdateCustomerAsync(result.EnrichedCustomer);
            }
            return result;
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
            try
            {
                var customer = new Customer { CustomerCode = row.CustomerCode, CustomerName = row.CustomerName };
                var enrichmentResult = await _customerEnrichmentService.EnrichAndCategorizeCustomerAsync(customer, row.Address);

                var enrichedCandidates = new List<Customer>();
                foreach (var candidate in enrichmentResult.Candidates)
                {
                    var enrichedCandidate = await _googlePlacesService.GetPlaceDetailsAsync(candidate.PlaceId!);
                    if (enrichedCandidate != null)
                    {
                        enrichedCandidate.CustomerName = candidate.CustomerName; // Preserve the name from the search result
                        enrichedCandidates.Add(enrichedCandidate);
                    }
                }
                importRow.Candidates = enrichedCandidates;

                if (enrichmentResult.EnrichedCustomer != null)
                {
                    importRow.SelectedPlaceId = enrichmentResult.EnrichedCustomer.PlaceId;
                }
            }
            catch (Exception ex)
            {
                // It's better to log the exception here if a logger is available
                importRow.Error = ex.Message;
            }
            return importRow;
        }

        public async Task<CustomerImportReport> CommitImportAsync(List<CustomerImportRow> importRows)
        {
            var report = new CustomerImportReport { TotalRows = importRows.Count };

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
                        db.Customers.Add(customer);
                    }

                    // Always update basic info from the spreadsheet
                    customer!.CustomerName = row.Dto.CustomerName;
                    customer.BusinessHours = row.Dto.BusinessHours;
                    customer.ContactNumber = row.Dto.ContactNumber;

                    // Only update address info if a candidate was selected
                    if (!string.IsNullOrWhiteSpace(row.SelectedPlaceId))
                    {
                        var selectedCandidate = row.Candidates.FirstOrDefault(c => c.PlaceId == row.SelectedPlaceId);
                        if (selectedCandidate != null)
                        {
                            customer.PlaceId = selectedCandidate.PlaceId;
                            customer.Latitude = selectedCandidate.Latitude;
                            customer.Longitude = selectedCandidate.Longitude;
                            customer.Street1 = selectedCandidate.Street1;
                            customer.Street2 = selectedCandidate.Street2;
                            customer.City = selectedCandidate.City;
                            customer.Province = selectedCandidate.Province;
                            customer.PostalCode = selectedCandidate.PostalCode;
                            customer.Country = selectedCandidate.Country;
                            customer.Address = selectedCandidate.Address;
                            customer.DestinationRegionCategory = selectedCandidate.DestinationRegionCategory;
                            customer.DestinationGroupCategory = selectedCandidate.DestinationGroupCategory;
                        }
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

        public async Task<List<Customer>> SearchFromGooglePlacesAsync(string query)
        {
            return await _googlePlacesService.SearchPlacesAsync(query);
        }
    }

}
