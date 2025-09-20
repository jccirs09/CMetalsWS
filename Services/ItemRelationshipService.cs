using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using System.IO;

namespace CMetalsWS.Services
{
    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports => Errors.Count;
        public List<string> Errors { get; } = new List<string>();
    }

    public class ItemRelationshipService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly InventoryService _inventory;

        public ItemRelationshipService(IDbContextFactory<ApplicationDbContext> dbContextFactory, InventoryService inventory)
        {
            _dbContextFactory = dbContextFactory;
            _inventory = inventory;
        }

        public async Task<List<ItemRelationship>> GetAsync(string parentItemId, CancellationToken ct = default)
        {
            using var db = _dbContextFactory.CreateDbContext();
            parentItemId = parentItemId?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(parentItemId))
                return new List<ItemRelationship>();

            return await db.ItemRelationships
                .Where(r => r.ParentItemId == parentItemId && r.Relation == "CoilToSheet")
                .OrderBy(r => r.ChildItemId)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<ItemRelationship?> GetParentAsync(string childItemId, CancellationToken ct = default)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            childItemId = childItemId?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(childItemId))
                return null;

            return await db.ItemRelationships
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ChildItemId == childItemId && r.Relation == "CoilToSheet", ct);
        }

        public async Task AddChildAsync(string parentItemId, string childItemId, CancellationToken ct = default)
        {
            using var db = _dbContextFactory.CreateDbContext();
            parentItemId = parentItemId?.Trim() ?? "";
            childItemId = childItemId?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(parentItemId) || string.IsNullOrWhiteSpace(childItemId))
                throw new InvalidOperationException("Item IDs are required.");
            if (parentItemId == childItemId)
                throw new InvalidOperationException("Parent and child cannot be the same.");

            bool exists = await db.ItemRelationships
                .AnyAsync(r => r.ParentItemId == parentItemId && r.ChildItemId == childItemId && r.Relation == "CoilToSheet", ct);

            if (!exists)
            {
                var items = await _inventory.GetByItemIdsAsync(new List<string> { parentItemId, childItemId }, ct);
                var parent = items.FirstOrDefault(i => i.ItemId == parentItemId);
                var child = items.FirstOrDefault(i => i.ItemId == childItemId);

                if (parent == null) throw new InvalidOperationException("Parent item not found.");
                if (child == null) throw new InvalidOperationException("Child item not found.");

                db.ItemRelationships.Add(new ItemRelationship
                {
                    ParentItemId = parentItemId,
                    ChildItemId = childItemId,
                    ParentItemDescription = parent.Description ?? string.Empty,
                    ChildItemDescription = child.Description ?? string.Empty,
                    Relation = "CoilToSheet"
                });
                await db.SaveChangesAsync(ct);
            }
        }

        public async Task RemoveAsync(int relId, CancellationToken ct = default)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var rel = await db.ItemRelationships.FindAsync(relId);
            if (rel != null)
            {
                db.ItemRelationships.Remove(rel);
                await db.SaveChangesAsync(ct);
            }
        }

        public async Task<ImportResult> ImportFromExcelAsync(Stream stream, CancellationToken ct = default)
        {
            var result = new ImportResult();
            var rows = stream.Query(useHeaderRow: true).ToList();
            result.TotalRows = rows.Count;

            foreach (var row in rows)
            {
                IDictionary<string, object> rowDict = row;
                try
                {
                    // ItemCode is always required.
                    if (!rowDict.TryGetValue("ItemCode", out var itemCodeObj) || itemCodeObj is null || string.IsNullOrWhiteSpace(itemCodeObj.ToString()))
                    {
                        result.Errors.Add("Skipping a row: 'ItemCode' column is missing or empty.");
                        continue;
                    }
                    var itemCode = itemCodeObj.ToString();

                    // CoilRelationship is optional. If it's not present or empty, just skip the row silently.
                    if (!rowDict.TryGetValue("CoilRelationship", out var coilRelationshipObj) || coilRelationshipObj is null || string.IsNullOrWhiteSpace(coilRelationshipObj.ToString()))
                    {
                        continue;
                    }
                    var coilRelationship = coilRelationshipObj.ToString();

                    // If we get here, both ItemCode and CoilRelationship are valid, so create the relationship.
                    await AddChildAsync(coilRelationship, itemCode, ct);
                    result.SuccessfulImports++;
                }
                catch (Exception ex)
                {
                    // We should still log errors if AddChildAsync fails (e.g., parent/child not found in inventory).
                    result.Errors.Add($"Failed to import relationship for item '{rowDict.TryGetValue("ItemCode", out var ic) ? ic : "N/A"}': {ex.Message}");
                }
            }

            return result;
        }

        public async Task<Dictionary<string, ItemRelationship>> GetParentsForChildrenAsync(List<string> childItemIds, CancellationToken ct = default)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            if (childItemIds == null || childItemIds.Count == 0)
                return new Dictionary<string, ItemRelationship>();

            return await db.ItemRelationships
                .AsNoTracking()
                .Where(r => childItemIds.Contains(r.ChildItemId) && r.Relation == "CoilToSheet")
                .ToDictionaryAsync(r => r.ChildItemId, r => r, ct);
        }
    }
}
