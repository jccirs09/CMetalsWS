using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
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
    }
}
