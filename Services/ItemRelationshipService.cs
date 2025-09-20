using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using MudBlazor;

namespace CMetalsWS.Services
{
    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int Processed { get; set; }
        public int Updated { get; set; }
        public int Added { get; set; }
        public List<string> Errors { get; } = new List<string>();
    }

    public class ItemRelationshipService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ItemRelationshipService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<ItemRelationship?> GetByItemCodeAsync(string itemCode, CancellationToken ct = default)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            return await db.ItemRelationships.AsNoTracking().FirstOrDefaultAsync(i => i.ItemCode == itemCode, ct);
        }

        public async Task<List<ItemRelationship>> GetChildrenAsync(string parentItemCode, CancellationToken ct = default)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            return await db.ItemRelationships
                .AsNoTracking()
                .Where(i => i.CoilRelationship == parentItemCode)
                .OrderBy(i => i.ItemCode)
                .ToListAsync(ct);
        }

        public async Task<ItemRelationship?> GetParentAsync(string childItemCode, CancellationToken ct = default)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            var child = await db.ItemRelationships.AsNoTracking().FirstOrDefaultAsync(i => i.ItemCode == childItemCode, ct);
            if (child == null || string.IsNullOrEmpty(child.CoilRelationship))
            {
                return null;
            }
            return await db.ItemRelationships.AsNoTracking().FirstOrDefaultAsync(i => i.ItemCode == child.CoilRelationship, ct);
        }

        public async Task<ImportResult> ImportFromExcelAsync(Stream stream, CancellationToken ct = default)
        {
            var result = new ImportResult();
            var rows = stream.Query<ItemRelationship>(useHeaderRow: true).ToList();
            result.TotalRows = rows.Count;

            using var db = await _dbContextFactory.CreateDbContextAsync(ct);

            foreach (var row in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(row.ItemCode))
                    {
                        result.Errors.Add("Skipping a row: 'ItemCode' column is missing or empty.");
                        continue;
                    }

                    result.Processed++;
                    var existing = await db.ItemRelationships.FirstOrDefaultAsync(i => i.ItemCode == row.ItemCode, ct);
                    if (existing != null)
                    {
                        existing.Description = row.Description;
                        existing.CoilRelationship = string.IsNullOrWhiteSpace(row.CoilRelationship) ? null : row.CoilRelationship;
                        result.Updated++;
                    }
                    else
                    {
                        db.ItemRelationships.Add(new ItemRelationship
                        {
                            ItemCode = row.ItemCode,
                            Description = row.Description,
                            CoilRelationship = string.IsNullOrWhiteSpace(row.CoilRelationship) ? null : row.CoilRelationship
                        });
                        result.Added++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to import row for item '{row.ItemCode}': {ex.Message}");
                }
            }

            await db.SaveChangesAsync(ct);
            return result;
        }

        public async Task<Dictionary<string, ItemRelationship>> GetParentsForChildrenAsync(List<string> childItemCodes, CancellationToken ct = default)
        {
            var result = new Dictionary<string, ItemRelationship>();
            if (childItemCodes == null || !childItemCodes.Any())
            {
                return result;
            }

            using var db = await _dbContextFactory.CreateDbContextAsync(ct);

            // Find the master items for the given child codes
            var children = await db.ItemRelationships
                .AsNoTracking()
                .Where(i => childItemCodes.Contains(i.ItemCode))
                .ToListAsync(ct);

            // Get the codes of the parents
            var parentItemCodes = children
                .Where(c => !string.IsNullOrEmpty(c.CoilRelationship))
                .Select(c => c.CoilRelationship!)
                .Distinct()
                .ToList();

            if (!parentItemCodes.Any())
            {
                return result;
            }

            // Fetch all the parent items in one query
            var parents = await db.ItemRelationships
                .AsNoTracking()
                .Where(i => parentItemCodes.Contains(i.ItemCode))
                .ToDictionaryAsync(p => p.ItemCode, p => p, ct);

            // Map each child to its parent
            foreach (var child in children)
            {
                if (!string.IsNullOrEmpty(child.CoilRelationship) && parents.TryGetValue(child.CoilRelationship, out var parent))
                {
                    result[child.ItemCode] = parent;
                }
            }

            return result;
        }

        public async Task<(IEnumerable<ItemRelationship> Items, int Total)> GetAllAsync(string? searchString, int page, int pageSize, string? sortLabel, SortDirection sortDirection, CancellationToken ct = default)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            var query = db.ItemRelationships.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(i => i.ItemCode.Contains(searchString) || i.Description.Contains(searchString) || (i.CoilRelationship != null && i.CoilRelationship.Contains(searchString)));
            }

            switch (sortLabel)
            {
                case "item_code":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(i => i.ItemCode) : query.OrderByDescending(i => i.ItemCode);
                    break;
                case "description":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(i => i.Description) : query.OrderByDescending(i => i.Description);
                    break;
                case "coil_relationship":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(i => i.CoilRelationship) : query.OrderByDescending(i => i.CoilRelationship);
                    break;
                default:
                    query = query.OrderBy(i => i.ItemCode);
                    break;
            }

            var totalItems = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return (items, totalItems);
        }
    }
}
