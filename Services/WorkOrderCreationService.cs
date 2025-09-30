using System.Text.RegularExpressions;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services;

public class WorkOrderCreationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public WorkOrderCreationService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<(int SuccessCount, int FailCount)> CreateFromEligiblePickingListsAsync(int branchId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var itemsToSchedule = await db.PickingListItems
            .AsNoTracking()
            .Include(pli => pli.PickingList)
            .Include(pli => pli.Machine)
            .Where(pli =>
                pli.PickingList.BranchId == branchId &&
                pli.Status == PickingLineStatus.AssignedProduction &&
                pli.MachineId.HasValue &&
                pli.Machine != null &&
                (pli.Machine.Category == MachineCategory.CTL || pli.Machine.Category == MachineCategory.Slitter))
            .OrderBy(i => i.PickingList.ShipDate)
            .ThenBy(i => i.PickingList.Priority)
            .ToListAsync();

        if (!itemsToSchedule.Any())
        {
            Console.WriteLine("[WO Creation Service] No items to schedule.");
            return (0, 0);
        }

        var groupedByMachine = itemsToSchedule.GroupBy(i => i.MachineId!.Value).ToList();
        var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId);
        var branchCode = branch?.Code ?? "00";
        var woCounter = await db.WorkOrders.CountAsync(w => w.BranchId == branchId);

        var newWorkOrders = new List<WorkOrder>();
        var lastScheduleTimes = new Dictionary<int, DateTime>();
        var allocatedCoilWeights = new Dictionary<int, decimal>();
        var processedPliIds = new List<int>();

        foreach (var group in groupedByMachine)
        {
            var machineId = group.Key;
            var machine = group.First().Machine!;
            var itemsForMachine = group.ToList();

            while (itemsForMachine.Any())
            {
                var firstItem = itemsForMachine.First();
                var parentCoil = await FindParentCoilAsync(db, firstItem, machine.Category, allocatedCoilWeights);

                if (parentCoil == null)
                {
                    itemsForMachine.RemoveAt(0);
                    continue;
                }

                var parentSnap = parentCoil.Snapshot ?? 0m;
                if (parentSnap <= 0)
                {
                    itemsForMachine.RemoveAt(0);
                    continue;
                }

                var parentAllocated = allocatedCoilWeights.GetValueOrDefault(parentCoil.Id, 0m);
                var parentCoilAvailableWeight = parentSnap - parentAllocated;

                if (parentCoilAvailableWeight <= 0)
                {
                    itemsForMachine.RemoveAt(0);
                    continue;
                }

                if (!lastScheduleTimes.TryGetValue(machineId, out var lastEndTime))
                {
                    lastEndTime = await db.WorkOrders
                        .Where(wo => wo.MachineId == machineId && wo.ScheduledEndDate.HasValue)
                        .MaxAsync(wo => (DateTime?)wo.ScheduledEndDate) ?? DateTime.Today.AddHours(8);
                }
                var scheduleStart = lastEndTime.AddMinutes(15);

                var wo = new WorkOrder
                {
                    WorkOrderNumber = $"W{branchCode}{++woCounter:0000000}",
                    TagNumber = parentCoil.TagNumber ?? "FROM-SERVICE",
                    BranchId = branchId,
                    MachineId = machineId,
                    MachineCategory = machine.Category,
                    ParentItemId = parentCoil.ItemId,
                    ParentItemDescription = parentCoil.Description,
                    ParentItemWeight = parentCoil.Snapshot,
                    Instructions = "Created from Picking List.",
                    CreatedBy = "SYSTEM",
                    LastUpdatedBy = "SYSTEM",
                    ScheduledStartDate = scheduleStart,
                    Status = WorkOrderStatus.Pending,
                    Priority = WorkOrderPriority.Normal,
                    Items = new List<WorkOrderItem>()
                };

                decimal currentWoWeight = 0;
                var itemsAddedToThisWo = new List<PickingListItem>();

                foreach (var item in itemsForMachine)
                {
                    var itemWeight = item.Weight ?? 0m;
                    if (itemWeight <= 0) continue;

                    if (currentWoWeight + itemWeight <= parentCoilAvailableWeight)
                    {
                        wo.Items.Add(new WorkOrderItem
                        {
                            PickingListItemId = item.Id,
                            ItemCode = item.ItemId,
                            Description = item.ItemDescription,
                            SalesOrderNumber = item.PickingList.SalesOrderNumber,
                            CustomerName = item.PickingList.SoldTo,
                            OrderQuantity = item.Quantity,
                            OrderWeight = item.Weight,
                            Width = item.Width,
                            Length = item.Length,
                            Unit = item.Unit,
                            Status = WorkOrderItemStatus.Pending,
                            IsStockItem = false
                        });
                        currentWoWeight += itemWeight;
                        itemsAddedToThisWo.Add(item);
                    }
                }

                if (wo.Items.Any())
                {
                    wo.DueDate = itemsAddedToThisWo.Min(i => i.ScheduledShipDate);
                    var estimatedDuration = TimeSpan.FromHours(itemsAddedToThisWo.Count * 0.5);
                    wo.ScheduledEndDate = scheduleStart.Add(estimatedDuration);
                    lastScheduleTimes[machineId] = wo.ScheduledEndDate.Value;

                    allocatedCoilWeights[parentCoil.Id] = allocatedCoilWeights.GetValueOrDefault(parentCoil.Id, 0m) + currentWoWeight;
                    newWorkOrders.Add(wo);
                    processedPliIds.AddRange(itemsAddedToThisWo.Select(i => i.Id));
                    itemsForMachine.RemoveAll(i => itemsAddedToThisWo.Contains(i));
                }
                else
                {
                    itemsForMachine.RemoveAt(0);
                }
            }
        }

        if (newWorkOrders.Any())
        {
            db.WorkOrders.AddRange(newWorkOrders);

            var pliToUpdate = await db.PickingListItems
                .Where(pli => processedPliIds.Contains(pli.Id))
                .ToListAsync();

            foreach (var pli in pliToUpdate)
            {
                pli.Status = PickingLineStatus.WorkOrder;
            }

            await db.SaveChangesAsync();
            return (newWorkOrders.Count, itemsToSchedule.Count - processedPliIds.Count);
        }

        return (0, itemsToSchedule.Count);
    }

    private async Task<InventoryItem?> FindParentCoilAsync(
        ApplicationDbContext db,
        PickingListItem itemToSchedule,
        MachineCategory category,
        IReadOnlyDictionary<int, decimal> allocatedWeights)
    {
        List<string> parentItemIds;

        if (category == MachineCategory.CTL)
        {
            var relationship = await db.ItemRelationships.AsNoTracking()
                .FirstOrDefaultAsync(ir => ir.ItemCode == itemToSchedule.ItemId);
            if (string.IsNullOrEmpty(relationship?.CoilRelationship)) return null;
            parentItemIds = new List<string> { relationship.CoilRelationship };
        }
        else if (category == MachineCategory.Slitter)
        {
            var id = itemToSchedule.ItemId;
            var baseId = NormalizeToBaseCoilId(id);
            parentItemIds = baseId == id ? new List<string> { id } : new List<string> { id, baseId };
        }
        else
        {
            return null;
        }

        var potentialCoils = await db.Set<InventoryItem>()
            .AsNoTracking()
            .Where(inv => parentItemIds.Contains(inv.ItemId) && inv.SnapshotUnit == "LBS" && inv.Snapshot > 0)
            .OrderByDescending(inv => inv.Snapshot)
            .ToListAsync();

        var firstItemWeight = itemToSchedule.Weight ?? 0m;

        foreach (var coil in potentialCoils)
        {
            var snap = coil.Snapshot ?? 0m;
            if (snap <= 0) continue;

            var allocated = allocatedWeights.GetValueOrDefault(coil.Id, 0m);
            var available = snap - allocated;
            if (available >= firstItemWeight)
            {
                return coil;
            }
        }
        return null;
    }

    private string NormalizeToBaseCoilId(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return itemId;
        var m = Regex.Match(itemId, @"^(.*?)-\d+(\.\d+)?$");
        if (m.Success) return m.Groups[1].Value;
        return itemId;
    }
}