using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class ItemRelationshipService
    {
        private readonly ApplicationDbContext _db;
        public ItemRelationshipService(ApplicationDbContext db) => _db = db;

        public async Task<(string? parentDesc, List<(int relId, string childId, string? childDesc)> children)>
            GetAsync(string parentItemId, CancellationToken ct = default)
        {
            parentItemId = parentItemId?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(parentItemId))
                return (null, new());

            var parentDesc = await _db.InventoryItems
                .Where(i => i.ItemId == parentItemId)
                .GroupBy(i => i.ItemId)
                .Select(g => g.Max(x => x.Description))
                .FirstOrDefaultAsync(ct);

            var rels = await _db.ItemRelationships
                .Where(r => r.ParentItemId == parentItemId && r.Relation == "CoilToSheet")
                .OrderBy(r => r.ChildItemId)
                .ToListAsync(ct);

            var childIds = rels.Select(r => r.ChildItemId).Distinct().ToList();
            var childDescMap = await _db.InventoryItems
                .Where(i => childIds.Contains(i.ItemId))
                .GroupBy(i => i.ItemId)
                .Select(g => new { g.Key, Desc = g.Max(x => x.Description) })
                .ToDictionaryAsync(x => x.Key, x => x.Desc, ct);

            var children = rels
                .Select(r => (r.Id, r.ChildItemId, childDescMap.TryGetValue(r.ChildItemId, out var d) ? d : null))
                .ToList();

            return (parentDesc, children);
        }

        public async Task AddChildAsync(string parentItemId, string childItemId, CancellationToken ct = default)
        {
            parentItemId = parentItemId?.Trim() ?? "";
            childItemId = childItemId?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(parentItemId) || string.IsNullOrWhiteSpace(childItemId))
                throw new InvalidOperationException("Item IDs are required.");
            if (parentItemId == childItemId)
                throw new InvalidOperationException("Parent and child cannot be the same.");

            bool exists = await _db.ItemRelationships
                .AnyAsync(r => r.ParentItemId == parentItemId && r.ChildItemId == childItemId && r.Relation == "CoilToSheet", ct);

            if (!exists)
            {
                _db.ItemRelationships.Add(new ItemRelationship
                {
                    ParentItemId = parentItemId,
                    ChildItemId = childItemId,
                    Relation = "CoilToSheet"
                });
                await _db.SaveChangesAsync(ct);
            }
        }

        public async Task RemoveAsync(int relId, CancellationToken ct = default)
        {
            var rel = await _db.ItemRelationships.FindAsync(relId);
            if (rel != null)
            {
                _db.ItemRelationships.Remove(rel);
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
