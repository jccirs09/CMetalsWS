using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class InventoryService
    {
        private readonly ApplicationDbContext _db;
        public InventoryService(ApplicationDbContext db) => _db = db;

        public async Task<InventoryItem?> GetByTagNumberAsync(string tagNumber, CancellationToken ct = default)
        {
            tagNumber = tagNumber?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(tagNumber))
                return null;

            return await _db.InventoryItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.TagNumber == tagNumber, ct);
        }

        public async Task<List<InventoryItem>> GetByItemIdsAsync(List<string> itemIds, CancellationToken ct = default)
        {
            if (itemIds == null || itemIds.Count == 0)
                return new List<InventoryItem>();

            return await _db.InventoryItems
                .AsNoTracking()
                .Where(i => itemIds.Contains(i.ItemId))
                .ToListAsync(ct);
        }

        // Upsert by (BranchId, TagNumber)
        public async Task<(int inserted, int updated)> UpsertAsync(IEnumerable<InventoryItem> rows, CancellationToken ct = default)
        {
            if (rows == null) return (0, 0);

            var list = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.TagNumber))
                .Select(r =>
                {
                    r.ItemId = (r.ItemId ?? "").Trim();
                    r.TagNumber = r.TagNumber!.Trim();
                    r.Description = r.Description?.Trim() ?? string.Empty;
                    r.Location = r.Location?.Trim();
                    return r;
                })
                .ToList();

            if (list.Count == 0) return (0, 0);

            var branchId = list.First().BranchId;
            var tags = list.Select(x => x.TagNumber).Distinct().ToList();

            var existing = await _db.InventoryItems
                .Where(i => i.BranchId == branchId && tags.Contains(i.TagNumber!))
                .ToDictionaryAsync(e => e.TagNumber!, ct);

            int ins = 0, upd = 0;

            foreach (var incoming in list)
            {
                if (!existing.TryGetValue(incoming.TagNumber!, out var current))
                {
                    _db.InventoryItems.Add(new InventoryItem
                    {
                        BranchId = incoming.BranchId,
                        ItemId = incoming.ItemId,
                        Description = incoming.Description,
                        TagNumber = incoming.TagNumber,
                        Width = incoming.Width,
                        Length = incoming.Length,
                        Snapshot = incoming.Snapshot,
                        Location = incoming.Location
                    });
                    ins++;
                }
                else
                {
                    current.ItemId = incoming.ItemId;
                    current.Description = incoming.Description;
                    current.Width = incoming.Width;
                    current.Length = incoming.Length;
                    current.Snapshot = incoming.Snapshot;
                    current.Location = incoming.Location;
                    upd++;
                }
            }

            await _db.SaveChangesAsync(ct);
            return (ins, upd);
        }
    }
}
