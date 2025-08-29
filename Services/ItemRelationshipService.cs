using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class ItemRelationshipService
    {
        private readonly ApplicationDbContext _db;
        private readonly InventoryService _inventory;

        public ItemRelationshipService(ApplicationDbContext db, InventoryService inventory)
        {
            _db = db;
            _inventory = inventory;
        }

        public async Task<List<ItemRelationship>> GetAsync(string parentItemId, CancellationToken ct = default)
        {
            parentItemId = parentItemId?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(parentItemId))
                return new List<ItemRelationship>();

            return await _db.ItemRelationships
                .Where(r => r.ParentItemId == parentItemId && r.Relation == "CoilToSheet")
                .OrderBy(r => r.ChildItemId)
                .AsNoTracking()
                .ToListAsync(ct);
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
                var items = await _inventory.GetByItemIdsAsync(new List<string> { parentItemId, childItemId }, ct);
                var parent = items.FirstOrDefault(i => i.ItemId == parentItemId);
                var child = items.FirstOrDefault(i => i.ItemId == childItemId);

                if (parent == null) throw new InvalidOperationException("Parent item not found.");
                if (child == null) throw new InvalidOperationException("Child item not found.");

                _db.ItemRelationships.Add(new ItemRelationship
                {
                    ParentItemId = parentItemId,
                    ChildItemId = childItemId,
                    ParentItemDescription = parent.Description ?? string.Empty,
                    ChildItemDescription = child.Description ?? string.Empty,
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
