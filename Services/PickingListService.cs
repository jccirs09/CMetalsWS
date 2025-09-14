using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class PickingListService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IPickingListImportService _importService;
        private readonly IPdfParsingService _parsingService;
        private readonly IConfiguration _configuration;

        public PickingListService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IPickingListImportService importService,
            IPdfParsingService parsingService,
            IConfiguration configuration)
        {
            _dbContextFactory = dbContextFactory;
            _importService = importService;
            _parsingService = parsingService;
            _configuration = configuration;
        }

        public async Task<List<PickingList>> GetAsync(int? branchId = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var query = db.PickingLists
                .Include(p => p.Branch)
                .Include(p => p.Customer)
                .Include(p => p.Items).ThenInclude(i => i.Machine)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(p => p.BranchId == branchId.Value);

            return await query.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<List<PickingList>> GetAvailableForLoadingAsync(int? branchId = null)
        {
            using var db = _dbContextFactory.CreateDbContext();

            var loadedItemIds = await db.LoadItems
                .Select(li => li.PickingListItemId)
                .Distinct()
                .ToListAsync();

            var query = db.PickingLists
                .Include(p => p.Customer)
                .Include(p => p.Items)
                .Include(p => p.Branch)
                .Where(p => p.Status == PickingListStatus.Completed &&
                             p.Items.Any(i => !loadedItemIds.Contains(i.Id)))
                .AsNoTracking();

            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            return await query.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<PickingList?> GetByIdAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.PickingLists
                .Include(p => p.Customer)
                .Include(p => p.Items).ThenInclude(i => i.Machine)
                .Include(p => p.Branch)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateAsync(PickingList model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            model.Status = PickingListStatus.Pending;
            foreach (var item in model.Items)
                item.Status = item.Status == 0 ? PickingLineStatus.Pending : item.Status;

            db.PickingLists.Add(model);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(PickingList model)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var existing = await db.PickingLists
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == model.Id);
            if (existing is null) return;

            existing.SalesOrderNumber = model.SalesOrderNumber;
            existing.BranchId = model.BranchId;
            existing.CustomerId = model.CustomerId;
            existing.Status = model.Status;

            var incomingIds = model.Items.Select(i => i.Id).ToHashSet();
            var toDelete = existing.Items.Where(i => !incomingIds.Contains(i.Id)).ToList();
            if (toDelete.Count > 0)
                db.PickingListItems.RemoveRange(toDelete);
            var existingItems = existing.Items.ToDictionary(i => i.Id);

            foreach (var item in model.Items)
            {
                if (item.Id == 0)
                {
                    existing.Items.Add(new PickingListItem
                    {
                        LineNumber = item.LineNumber,
                        ItemId = item.ItemId,
                        ItemDescription = item.ItemDescription,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        Width = item.Width,
                        Length = item.Length,
                        Weight = item.Weight,
                        MachineId = item.MachineId,
                        Status = item.Status == 0 ? PickingLineStatus.Pending : item.Status
                    });
                }
                else
                {
                    if (existingItems.TryGetValue(item.Id, out var tgt))
                    {
                        tgt.LineNumber = item.LineNumber;
                        tgt.ItemId = item.ItemId;
                        tgt.ItemDescription = item.ItemDescription;
                        tgt.Quantity = item.Quantity;
                        tgt.Unit = item.Unit;
                        tgt.Width = item.Width;
                        tgt.Length = item.Length;
                        tgt.Weight = item.Weight;
                        tgt.MachineId = item.MachineId;
                        tgt.Status = item.Status;
                        tgt.PulledQuantity = item.PulledQuantity;
                        tgt.PulledWeight = item.PulledWeight;
                    }
                }
            }

            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var entity = await db.PickingLists.FindAsync(id);
            if (entity != null)
            {
                db.PickingLists.Remove(entity);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdatePickingListStatusAsync(int id, PickingListStatus status, CancellationToken ct = default)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            var list = await db.PickingLists.FindAsync(new object[] { id }, cancellationToken: ct);
            if (list is null) return;

            list.Status = status;

            await db.SaveChangesAsync(ct);
        }

        private PickingListStatus CalculateStatus(ICollection<PickingListItem> items)
        {
            if (items.All(i => i.Status == PickingLineStatus.AssignedProduction))
                return PickingListStatus.Awaiting;
            if (items.All(i => i.Status == PickingLineStatus.AssignedPulling))
                return PickingListStatus.Awaiting;
            if (items.All(i => i.Status == PickingLineStatus.Completed))
                return PickingListStatus.Completed;
            if (items.Any(i => i.Status == PickingLineStatus.InProgress))
                return PickingListStatus.InProgress;

            return PickingListStatus.Pending;
        }

        public async Task SetLineStatusAsync(int itemId, PickingLineStatus status)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var item = await db.PickingListItems.Include(i => i.PickingList).ThenInclude(pl => pl.Items).FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null) return;

            item.Status = status;

            if (item.PickingList != null)
            {
                var newStatus = CalculateStatus(item.PickingList.Items);
                item.PickingList.Status = newStatus;
            }

            await db.SaveChangesAsync();
        }
        //TODO: Refactor this method to work with the new data model
        //public async Task ScheduleListAsync(int pickingListId, DateTime shipStart)
        //{
        //    using var db = _dbContextFactory.CreateDbContext();
        //    var pl = await db.PickingLists.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == pickingListId);
        //    if (pl is null) return;
        //    //pl.ShipDate = shipStart;
        //    await db.SaveChangesAsync();
        //}

        public async Task ScheduleLineAsync(int pickingListId, int lineId, DateTime shipStart)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var line = await db.PickingListItems.FirstOrDefaultAsync(i => i.Id == lineId && i.PickingListId == pickingListId);
            if (line is null) return;
            line.ScheduledShipDate = shipStart;
            await db.SaveChangesAsync();
        }

        public async Task<List<PickingListItem>> GetPendingItemsByItemIdsAsync(List<string> itemIds)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.PickingListItems
                .Include(i => i.PickingList!)
                .ThenInclude(pl => pl.Customer)
                .Where(i => i.PickingList != null && (i.Status == PickingLineStatus.Pending || i.Status == PickingLineStatus.Awaiting) && itemIds.Contains(i.ItemId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PickingListItem>> GetPendingItemsByParentItemIdAsync(string parentItemId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return await db.PickingListItems
                .Include(i => i.PickingList!)
                .ThenInclude(pl => pl.Customer)
                .Where(i => i.PickingList != null && (i.Status == PickingLineStatus.Pending || i.Status == PickingLineStatus.Awaiting) && i.ItemId == parentItemId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PickingList>> GetPendingPullingOrdersAsync(int? branchId = null)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var query = db.PickingLists
                .Include(p => p.Items)
                .Where(p => p.Status == PickingListStatus.Pending &&
                            !p.Items.Any(i => db.WorkOrderItems.Any(wi => wi.PickingListItemId == i.Id)))
                .AsNoTracking();

            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<PickingListItem>> GetPullingTasksAsync(int? branchId = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var query = db.PickingListItems
                .Include(i => i.PickingList)
                .Where(i => i.PickingList != null && i.Status == PickingLineStatus.AssignedPulling)
                .AsNoTracking();

            if (branchId.HasValue)
            {
                query = query.Where(i => i.PickingList!.BranchId == branchId.Value);
            }

            return await query.OrderBy(i => i.PickingList!.Id).ToListAsync();
        }

        public async Task<List<PickingListItem>> GetSheetPullingQueueAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.PickingListItems
                .Include(i => i.PickingList)
                .Include(i => i.Machine)
                .Where(i => i.Status == PickingLineStatus.AssignedPulling &&
                            i.Machine != null &&
                            i.Machine.Category == MachineCategory.Sheet)
                .OrderBy(i => i.PickingList.Priority)
                .ThenBy(i => i.PickingList.ShipDate)
                .ToListAsync();
        }

        public async Task UpdatePulledQuantitiesAsync(int itemId, decimal? pulledQuantity, decimal pulledWeight)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var item = await db.PickingListItems.FindAsync(itemId);
            if (item == null) return;

            item.PulledQuantity = pulledQuantity;
            item.PulledWeight = pulledWeight;

            await db.SaveChangesAsync();
        }

        public async Task<int> UpsertFromParsedDataAsync(int branchId, PickingList parsedList, List<PickingListItem> parsedItems)
        {
            // Propagate the main ship date to all line items if it exists.
            if (parsedList.ShipDate.HasValue)
            {
                foreach (var item in parsedItems)
                {
                    item.ScheduledShipDate = parsedList.ShipDate;
                }
            }

            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existingList = await db.PickingLists
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.BranchId == branchId && p.SalesOrderNumber == parsedList.SalesOrderNumber);

            if (existingList == null)
            {
                // Create new list
                parsedList.BranchId = branchId;
                parsedList.Status = PickingListStatus.Pending; // Or some other default
                parsedList.Items = parsedItems;
                db.PickingLists.Add(parsedList);
            }
            else
            {
                // Update existing list header by manually mapping properties
                // This avoids trying to change the primary key, which causes an exception.
                existingList.OrderDate = parsedList.OrderDate;
                existingList.ShipDate = parsedList.ShipDate;
                existingList.SoldTo = parsedList.SoldTo;
                existingList.ShipTo = parsedList.ShipTo;
                existingList.SalesRep = parsedList.SalesRep;
                existingList.ShippingVia = parsedList.ShippingVia;
                existingList.FOB = parsedList.FOB;
                existingList.Buyer = parsedList.Buyer;
                existingList.PrintDateTime = parsedList.PrintDateTime;
                existingList.TotalWeight = parsedList.TotalWeight;
                // We don't update BranchId or SalesOrderNumber as they are the keys for the lookup.

                var existingItemsDict = existingList.Items.ToDictionary(i => (i.LineNumber, i.ItemId));
                var parsedItemsDict = parsedItems.ToDictionary(i => (i.LineNumber, i.ItemId));

                // Items to delete
                var itemsToDelete = existingList.Items.Where(i => !parsedItemsDict.ContainsKey((i.LineNumber, i.ItemId))).ToList();
                db.PickingListItems.RemoveRange(itemsToDelete);

                foreach (var parsedItem in parsedItems)
                {
                    if (existingItemsDict.TryGetValue((parsedItem.LineNumber, parsedItem.ItemId), out var existingItem))
                    {
                        // Update existing item, preserving MachineId
                        var originalMachineId = existingItem.MachineId;
                        db.Entry(existingItem).CurrentValues.SetValues(parsedItem);
                        existingItem.MachineId = originalMachineId;
                    }
                    else
                    {
                        // Add new item
                        existingList.Items.Add(parsedItem);
                    }
                }
            }

            await db.SaveChangesAsync();
            return existingList?.Id ?? parsedList.Id;
        }

        public async Task UpdateMachineAssignmentsAsync(IEnumerable<PickingListItem> itemsToUpdate)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            foreach (var item in itemsToUpdate)
            {
                var trackedItem = await db.PickingListItems.FindAsync(item.Id);
                if (trackedItem != null)
                {
                    trackedItem.MachineId = item.MachineId;
                }
            }
            await db.SaveChangesAsync();
        }

        public async Task ReParseAsync(int pickingListId)
        {
            var latestImport = await _importService.GetLatestImportByPickingListIdAsync(pickingListId);
            if (latestImport == null || !System.IO.File.Exists(latestImport.SourcePdfPath))
            {
                throw new InvalidOperationException("Could not find the original PDF file to re-parse.");
            }

            var pickingList = await GetByIdAsync(pickingListId);
            if (pickingList == null)
            {
                throw new InvalidOperationException($"Picking list with ID {pickingListId} not found.");
            }

            var importGuid = Guid.NewGuid();
            var imagesDir = System.IO.Path.Combine("wwwroot", "uploads", "pickinglists", importGuid.ToString());
            var newImport = await _importService.CreateImportAsync(pickingList.BranchId, latestImport.SourcePdfPath, imagesDir, _configuration.GetValue<string>("OpenAI:Model") ?? "gpt-4o-mini");

            try
            {
                var imagePaths = await _parsingService.ConvertPdfToImagesAsync(latestImport.SourcePdfPath, importGuid);
                var (parsedList, parsedItems) = await _parsingService.ParsePickingListAsync(imagePaths);

                var newPickingListId = await UpsertFromParsedDataAsync(pickingList.BranchId, parsedList, parsedItems);

                var rawJson = System.Text.Json.JsonSerializer.Serialize(new { header = parsedList, items = parsedItems });
                await _importService.UpdateImportSuccessAsync(newImport.Id, newPickingListId, rawJson);
            }
            catch (Exception ex)
            {
                await _importService.UpdateImportFailedAsync(newImport.Id, ex.ToString());
                throw; // Re-throw to notify the caller
            }
        }
    }
}
