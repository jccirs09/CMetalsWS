using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CMetalsWS.Services
{
    public class PickingListService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IPickingListImportService _importService;
        private readonly IPdfParsingService _parsingService;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ScheduleHub> _hubContext;

        public PickingListService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IPickingListImportService importService,
            IPdfParsingService parsingService,
            IConfiguration configuration,
            IHubContext<ScheduleHub> hubContext)
        {
            _dbContextFactory = dbContextFactory;
            _importService = importService;
            _parsingService = parsingService;
            _configuration = configuration;
            _hubContext = hubContext;
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

        public async Task UpdatePickingListStatusAsync(int id)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var list = await db.PickingLists
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (list is null) return;

            if (list.Items.All(i => i.Status == PickingLineStatus.AssignedProduction))
                list.Status = PickingListStatus.Awaiting;
            else if (list.Items.All(i => i.Status == PickingLineStatus.AssignedPulling))
                list.Status = PickingListStatus.Awaiting;
            else if (list.Items.All(i => i.Status == PickingLineStatus.Completed))
                list.Status = PickingListStatus.Completed;
            else if (list.Items.Any(i => i.Status == PickingLineStatus.InProgress))
                list.Status = PickingListStatus.InProgress;
            else
                list.Status = PickingListStatus.Pending;

            await db.SaveChangesAsync();
        }

        public async Task SetLineStatusAsync(int itemId, PickingLineStatus status)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var item = await db.PickingListItems.FindAsync(itemId);
            if (item == null) return;
            item.Status = status;
            await db.SaveChangesAsync();
            await UpdatePickingListStatusAsync(item.PickingListId);
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

        public async Task<List<PickingListItem>> GetSheetPullingQueueAsync(int? machineId = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var query = db.PickingListItems
                .Include(i => i.PickingList)
                .Include(i => i.Machine)
                .Where(i => i.Status == PickingLineStatus.AssignedPulling &&
                            i.Machine != null &&
                            i.Machine.Category == MachineCategory.Sheet);

            if (machineId.HasValue)
            {
                query = query.Where(i => i.MachineId == machineId.Value);
            }

            return await query.OrderBy(i => i.ScheduledProcessingDate)
                .ThenBy(i => i.PickingList.Priority)
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

        public async Task<bool> HasChangesAsync(int branchId, PickingList parsedList, List<PickingListItem> parsedItems)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existingList = await db.PickingLists
                .Include(p => p.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BranchId == branchId && p.SalesOrderNumber == parsedList.SalesOrderNumber);

            if (existingList == null)
            {
                return true; // It's a new list, so it's a change.
            }

            parsedList.Items = parsedItems;
            return !AreEqual(existingList, parsedList);
        }

        public async Task<int> UpsertFromParsedDataAsync(int branchId, string userId, PickingList parsedList, List<PickingListItem> parsedItems)
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
                if (parsedList.ShipDate.HasValue)
                {
                    var maxPriority = await db.PickingLists
                        .Where(p => p.ShipDate.HasValue && p.ShipDate.Value.Date == parsedList.ShipDate.Value.Date)
                        .MaxAsync(p => (int?)p.Priority) ?? 0;
                    parsedList.Priority = maxPriority + 1;
                }
                // If no ship date, it will use the default priority of 99.

                parsedList.BranchId = branchId;
                parsedList.Status = PickingListStatus.Pending; // Or some other default
                parsedList.Items = parsedItems;
                parsedList.ScannedById = userId;
                parsedList.ScannedDate = DateTime.UtcNow;
                parsedList.ModifiedById = userId;
                parsedList.ModifiedDate = DateTime.UtcNow;
                db.PickingLists.Add(parsedList);
            }
            else
            {
                // Update existing list
                existingList.ModifiedById = userId;
                existingList.ModifiedDate = DateTime.UtcNow;

                // Update existing list header by manually mapping properties
                // This avoids trying to change the primary key, which causes an exception.
                existingList.OrderDate = parsedList.OrderDate;
                existingList.ShipDate = parsedList.ShipDate;
                existingList.SoldTo = parsedList.SoldTo;
                existingList.ShipTo = parsedList.ShipTo;
                existingList.SalesRep = parsedList.SalesRep;
                existingList.DestinationRegionId = parsedList.DestinationRegionId;
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
                        // Update existing item, preserving Id
                        existingItem.MachineId = parsedItem.MachineId;
                        existingItem.LineNumber = parsedItem.LineNumber;
                        existingItem.ItemId = parsedItem.ItemId;
                        existingItem.ItemDescription = parsedItem.ItemDescription;
                        existingItem.Quantity = parsedItem.Quantity;
                        existingItem.Unit = parsedItem.Unit;
                        existingItem.Width = parsedItem.Width;
                        existingItem.Length = parsedItem.Length;
                        existingItem.Weight = parsedItem.Weight;
                        existingItem.Status = parsedItem.Status;
                        existingItem.ScheduledShipDate = parsedItem.ScheduledShipDate;
                    }
                    else
                    {
                        // Add new item
                        existingList.Items.Add(parsedItem);
                    }
                }
            }

            await db.SaveChangesAsync();
            var updatedListId = existingList?.Id ?? parsedList.Id;
            await _hubContext.Clients.All.SendAsync("PickingListUpdated", updatedListId);
            return updatedListId;
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

        public async Task ReParseAsync(int pickingListId, string userId)
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

            var newImport = await _importService.CreateImportAsync(pickingList.BranchId, latestImport.SourcePdfPath, "", "PdfPig");

            try
            {
                var pdfBytes = await File.ReadAllBytesAsync(latestImport.SourcePdfPath);
                var (parsedList, parsedItems) = await _parsingService.ParsePickingListAsync(pdfBytes);

                var newPickingListId = await UpsertFromParsedDataAsync(pickingList.BranchId, userId, parsedList, parsedItems);

                var rawJson = System.Text.Json.JsonSerializer.Serialize(new { header = parsedList, items = parsedItems });
                await _importService.UpdateImportSuccessAsync(newImport.Id, newPickingListId, rawJson);
            }
            catch (Exception ex)
            {
                await _importService.UpdateImportFailedAsync(newImport.Id, ex.ToString());
                throw; // Re-throw to notify the caller
            }
        }

        public bool AreEqual(PickingList listA, PickingList listB)
        {
            if (listA == null || listB == null)
            {
                return listA == listB;
            }

            // Compare header properties
            if (listA.SalesOrderNumber != listB.SalesOrderNumber ||
                listA.SoldTo?.Trim() != listB.SoldTo?.Trim() ||
                listA.ShipTo?.Trim() != listB.ShipTo?.Trim() ||
                listA.OrderDate?.Date != listB.OrderDate?.Date ||
                listA.ShipDate?.Date != listB.ShipDate?.Date ||
                Math.Abs(listA.TotalWeight - listB.TotalWeight) > 0.001m)
            {
                return false;
            }

            // Compare items
            if (listA.Items.Count != listB.Items.Count)
            {
                return false;
            }

            var itemsA = listA.Items.OrderBy(i => i.LineNumber).ThenBy(i => i.ItemId).ToList();
            var itemsB = listB.Items.OrderBy(i => i.LineNumber).ThenBy(i => i.ItemId).ToList();

            for (int i = 0; i < itemsA.Count; i++)
            {
                var itemA = itemsA[i];
                var itemB = itemsB[i];

                if (itemA.LineNumber != itemB.LineNumber ||
                    itemA.ItemId != itemB.ItemId ||
                    itemA.ItemDescription?.Trim() != itemB.ItemDescription?.Trim() ||
                    Math.Abs(itemA.Quantity - itemB.Quantity) > 0.001m ||
                    itemA.Unit?.Trim() != itemB.Unit?.Trim() ||
                    itemA.Width != itemB.Width ||
                    itemA.Length != itemB.Length ||
                    Math.Abs((itemA.Weight ?? 0) - (itemB.Weight ?? 0)) > 0.001m)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task UpdatePullingQueueAsync(List<PickingListItem> items)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var itemIds = items.Select(i => i.Id).ToList();
            var trackedItems = await db.PickingListItems.Include(i => i.PickingList).Where(i => itemIds.Contains(i.Id)).ToListAsync();
            var trackedItemsDict = trackedItems.ToDictionary(i => i.Id);

            foreach (var item in items)
            {
                if (trackedItemsDict.TryGetValue(item.Id, out var trackedItem))
                {
                    trackedItem.MachineId = item.MachineId;
                    if (trackedItem.PickingList != null)
                    {
                        trackedItem.PickingList.Priority = item.PickingList.Priority;
                    }
                }
            }
            await db.SaveChangesAsync();
        }

        public async Task<List<PickingList>> GetSheetPullingQueueListsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var pickingListIds = await db.PickingListItems
                .Where(i => i.Status == PickingLineStatus.AssignedPulling &&
                            i.Machine != null &&
                            i.Machine.Category == MachineCategory.Sheet)
                .Select(i => i.PickingListId)
                .Distinct()
                .ToListAsync();

            return await db.PickingLists
                .Include(p => p.Customer)
                .Include(p => p.Items)
                .ThenInclude(i => i.Machine)
                .Where(p => pickingListIds.Contains(p.Id))
                .OrderBy(p => p.ShipDate)
                .ThenBy(p => p.Priority)
                .ToListAsync();
        }

        public async Task UpdatePullingQueueOrderAsync(List<PickingList> orderedLists, int droppedListId, int newMachineId, DateTime newScheduledProcessingDate)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            // The list comes in with a global order set in the Priority field.
            // We need to convert this to a (machine, date)-scoped priority.

            var droppedListInMemory = orderedLists.FirstOrDefault(l => l.Id == droppedListId);
            if (droppedListInMemory != null)
            {
                foreach (var item in droppedListInMemory.Items)
                {
                    item.MachineId = newMachineId;
                    item.ScheduledProcessingDate = newScheduledProcessingDate;
                }
            }

            var machineGroups = orderedLists.GroupBy(l => l.Items.FirstOrDefault(i => i.Machine?.Category == MachineCategory.Sheet)?.MachineId);

            foreach (var machineGroup in machineGroups)
            {
                var dateGroups = machineGroup.GroupBy(l => l.Items.FirstOrDefault(i => i.Machine?.Category == MachineCategory.Sheet)?.ScheduledProcessingDate?.Date);

                foreach (var dateGroup in dateGroups)
                {
                    int priority = 1; // Priority starts at 1 for each (machine, date) group
                    foreach (var list in dateGroup) // The group is already ordered by the global priority
                    {
                        var trackedList = await db.PickingLists.FindAsync(list.Id);
                        if (trackedList != null)
                        {
                            trackedList.Priority = priority++;
                        }
                    }
                }
            }

            // Update machine and date for all sheet items in the dropped list
            var trackedDroppedList = await db.PickingLists
                .Include(p => p.Items)
                .ThenInclude(i => i.Machine)
                .FirstOrDefaultAsync(p => p.Id == droppedListId);

            if (trackedDroppedList != null)
            {
                foreach (var item in trackedDroppedList.Items)
                {
                    if (item.Machine?.Category == MachineCategory.Sheet)
                    {
                        item.MachineId = newMachineId;
                        item.ScheduledProcessingDate = newScheduledProcessingDate;
                    }
                }
            }

            await db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("PickingListUpdated", droppedListId);
        }
    }
}
