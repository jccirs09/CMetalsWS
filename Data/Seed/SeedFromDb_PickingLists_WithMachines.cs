using System.Globalization;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Data.Seed;

public static class SeedFromDb_PickingLists_WithMachines
{
    private const int TargetPickingLists = 200;
    private const int MinLines = 2;
    private const int MaxLines = 6;
    private const int ShipHorizonDays = 60;
    public static async Task RunAsync(ApplicationDbContext db, int branchId)
    {
        // ----- Load sources from DB -----
        var customers = await db.Set<Customer>()
            .AsNoTracking()
            .Where(c => c.Active && !string.IsNullOrWhiteSpace(c.CustomerName))
            .ToListAsync(); // name / region fields exist here
        if (customers.Count == 0) return; // nothing to seed

        var inventory = await db.Set<InventoryItem>()
            .AsNoTracking()
            .Where(i => !string.IsNullOrWhiteSpace(i.ItemId))
            .ToListAsync(); // Width/Length/Snapshot/SnapshotUnit
        if (inventory.Count == 0) return;

        var regions = await db.Set<DestinationRegion>()
            .AsNoTracking()
            .Select(r => r.Id)
            .ToListAsync();
        if (regions.Count == 0) regions = Enumerable.Range(1, 4).ToList();

        // Machines by category (don’t assume IDs → use Category)
        var coilMachines = await db.Set<Machine>().AsNoTracking()
            .Where(m => m.BranchId == branchId && m.Category == MachineCategory.Coil)
            .Select(m => m.Id).ToListAsync();
        var slitterMachines = await db.Set<Machine>().AsNoTracking()
            .Where(m => m.BranchId == branchId && m.Category == MachineCategory.Slitter)
            .Select(m => m.Id).ToListAsync();
        var ctlMachines = await db.Set<Machine>().AsNoTracking()
            .Where(m => m.BranchId == branchId && m.Category == MachineCategory.CTL)
            .Select(m => m.Id).ToListAsync();
        var sheetMachines = await db.Set<Machine>().AsNoTracking()
            .Where(m => m.BranchId == branchId && m.Category == MachineCategory.Sheet)
            .Select(m => m.Id).ToListAsync();

        // ----- Count and figure out how many to create -----
        var existing = await db.PickingLists.Where(p => p.BranchId == branchId).CountAsync();
        var toMake = Math.Max(0, TargetPickingLists - existing);
        if (toMake == 0) return;

        var rng = new Random();
        var today = DateTime.Today;
        var reps = new[] { "CRAIG MUDIE", "DYLAN WILLIAMS", "SPENCER CHAPMAN", "NICOLE HARRIS", "JACK LAM" };
        var buyers = new[] { "JANE", "STEVE ARMITAGE", "ROBYN LOURENCO", "PROCUREMENT" };

        int soBase = 39250000 + rng.Next(1000);
        var ci = CultureInfo.InvariantCulture;

        for (int i = 0; i < toMake; i++)
        {
            var cust = customers[rng.Next(customers.Count)];
            var shipDate = today.AddDays(rng.Next(0, ShipHorizonDays + 1));
            var orderDate = shipDate.AddDays(-rng.Next(1, 7));
            var printAt = orderDate.AddHours(rng.Next(8, 17)).AddMinutes(rng.Next(0, 60));
            var so = (soBase + i).ToString(ci);

            var pl = new PickingList
            {
                BranchId = branchId,
                SalesOrderNumber = so,
                OrderDate = orderDate,
                ShipDate = shipDate,
                PrintDateTime = printAt,
                CustomerId = cust.Id,
                SoldTo = cust.CustomerName,
                ShipTo = BuildShipTo(cust),
                SalesRep = reps[rng.Next(reps.Length)],
                Buyer = buyers[rng.Next(buyers.Length)],
                DestinationRegionId = cust.DestinationRegionId ?? regions[rng.Next(regions.Count)],
                Status = PickingListStatus.Pending,
                Priority = 99
            };

            int lineCount = rng.Next(MinLines, MaxLines + 1);
            var picked = PickRandom(inventory, lineCount, rng);

            var items = new List<PickingListItem>(lineCount);
            var perSheetMap = new Dictionary<PickingListItem, decimal>();
            decimal totalWeight = 0m;
            int ln = 1;

            foreach (var inv in picked)
            {
                var hasLen = inv.Length.HasValue && inv.Length.Value > 0m;
                var hasWid = inv.Width.HasValue && inv.Width.Value > 0m;

                string unit;
                decimal qty;
                decimal weight;
                decimal perSheet = 0;

                if (hasLen)
                {
                    // SHEET/CTL → PCS with estimated weight (rough steel math)
                    unit = "PCS";
                    qty = (inv.SnapshotUnit?.Equals("PCS", StringComparison.OrdinalIgnoreCase) == true && inv.Snapshot is > 0)
                        ? Math.Max(1, Math.Round(inv.Snapshot.Value))
                        : rng.Next(10, 151);

                    var thickness = GuessThickness(inv.Description);
                    var density = 0.283m; // lb/in^3
                    var w = inv.Width ?? 48m;
                    var l = inv.Length ?? 120m;
                    perSheet = w * l * thickness * density;
                    weight = Math.Max(1m, Math.Round(perSheet * qty, 3));
                }
                else
                {
                    // COIL/SLITTER → LBS
                    unit = "LBS";
                    if (inv.SnapshotUnit?.Equals("LBS", StringComparison.OrdinalIgnoreCase) == true && inv.Snapshot is > 500)
                    {
                        var baseLbs = inv.Snapshot!.Value;
                        var wiggle = rng.Next(-500, 501);
                        weight = Math.Max(800, Math.Round(baseLbs + wiggle, 0));
                    }
                    else
                    {
                        if (hasWid && IsStdCoilWidth(inv.Width!.Value))
                            weight = rng.Next(3000, 14001);
                        else
                            weight = rng.Next(1500, 9001);
                    }
                    qty = weight;
                }

                int machineId = RouteMachineId(
                    inv.Width, hasLen,
                    coilMachines, slitterMachines, ctlMachines, sheetMachines,
                    rng);

                var lineStatus = (pl.Status == PickingListStatus.Pending)
                    ? (coilMachines.Contains(machineId) || sheetMachines.Contains(machineId)
                        ? PickingLineStatus.AssignedPulling
                        : PickingLineStatus.AssignedProduction)
                    : PickingLineStatus.Pending;

                var item = new PickingListItem
                {
                    LineNumber = ln++,
                    ItemId = inv.ItemId,
                    ItemDescription = BuildItemDescription(inv, hasLen, machineId),
                    Quantity = qty,
                    Unit = unit,
                    Width = hasWid ? inv.Width : null,
                    Length = hasLen ? inv.Length : null,
                    Weight = weight,
                    PulledQuantity = 0m,
                    PulledWeight = 0m,
                    Status = lineStatus,
                    ScheduledShipDate = shipDate,
                    ScheduledProcessingDate = BusinessDayMinusOne(shipDate),
                    MachineId = machineId
                };

                if (unit == "PCS") perSheetMap.Add(item, perSheet);
                items.Add(item);
                totalWeight += weight;
            }

            // Scale order to a realistic weight range
            if (totalWeight > 0)
            {
                var targetWeight = (decimal)rng.Next(2000, 25001);
                var factor = targetWeight / totalWeight;
                decimal finalTotalWeight = 0;
                foreach (var item in items)
                {
                    if (item.Unit == "LBS")
                    {
                        var newWeight = Math.Round((item.Weight ?? 0m) * factor, 0);
                        item.Weight = newWeight;
                        item.Quantity = newWeight;
                    }
                    else if (item.Unit == "PCS" && perSheetMap.TryGetValue(item, out var perSheet))
                    {
                        var newQty = Math.Max(1, Math.Round(item.Quantity * factor));
                        item.Quantity = newQty;
                        item.Weight = Math.Round(newQty * perSheet, 3);
                    }
                    finalTotalWeight += item.Weight ?? 0m;
                }
                totalWeight = finalTotalWeight;
            }

            pl.TotalWeight = Math.Round(totalWeight, 3);
            pl.RemainingWeight = pl.TotalWeight;

            // Upsert on (BranchId, SO)
            var existingPickingList = await db.PickingLists
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.BranchId == pl.BranchId && x.SalesOrderNumber == pl.SalesOrderNumber);

            if (existingPickingList is null)
            {
                pl.Items = items;
                db.PickingLists.Add(pl);
            }
            else
            {
                existingPickingList.OrderDate = pl.OrderDate;
                existingPickingList.ShipDate = pl.ShipDate;
                existingPickingList.PrintDateTime = pl.PrintDateTime;
                existingPickingList.CustomerId = pl.CustomerId;
                existingPickingList.SoldTo = pl.SoldTo;
                existingPickingList.ShipTo = pl.ShipTo;
                existingPickingList.SalesRep = pl.SalesRep;
                existingPickingList.Buyer = pl.Buyer;
                existingPickingList.DestinationRegionId = pl.DestinationRegionId;
                existingPickingList.Status = pl.Status;
                existingPickingList.Priority = pl.Priority;
                existingPickingList.TotalWeight = pl.TotalWeight;
                existingPickingList.RemainingWeight = pl.TotalWeight;
                existingPickingList.ModifiedDate = DateTime.UtcNow;

                db.PickingListItems.RemoveRange(existingPickingList.Items);
                foreach (var li in items)
                {
                    li.PickingListId = existingPickingList.Id;
                    db.PickingListItems.Add(li);
                }
            }
        }

        await db.SaveChangesAsync();

        await SeedWorkOrdersFromPickingListItems(db, branchId, rng);
    }

    private static async Task SeedWorkOrdersFromPickingListItems(ApplicationDbContext db, int branchId, Random rng)
    {
        var machineCategories = new[] { MachineCategory.CTL, MachineCategory.Slitter };

        var itemsToSchedule = await db.PickingListItems
            .Include(pli => pli.PickingList)
            .Include(pli => pli.Machine)
            .Where(pli =>
                pli.PickingList.BranchId == branchId &&
                pli.MachineId.HasValue &&
                pli.Machine != null &&
                machineCategories.Contains(pli.Machine.Category) &&
                !db.WorkOrderItems.Any(woi => woi.PickingListItemId == pli.Id))
            .AsNoTracking()
            .ToListAsync();

        if (!itemsToSchedule.Any()) return;

        var groupedByMachine = itemsToSchedule
            .GroupBy(i => i.MachineId!.Value)
            .ToList();

        var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId);
        var branchCode = branch?.Code ?? "00";
        var woCounter = await db.WorkOrders.CountAsync(w => w.BranchId == branchId);

        var newWorkOrders = new List<WorkOrder>();
        var lastScheduleTimes = new Dictionary<int, DateTime>();
        var allocatedCoilWeights = new Dictionary<int, decimal>();

        foreach (var group in groupedByMachine)
        {
            var machineId = group.Key;
            var machine = group.First().Machine!;
            var itemsForMachine = group
                .OrderBy(i => i.PickingList.ShipDate)
                .ThenBy(i => i.PickingList.Priority)
                .ToList();

            while (itemsForMachine.Any())
            {
                var firstItem = itemsForMachine.First();
                var parentCoil = await FindParentCoilAsync(db, firstItem, machine.Category, allocatedCoilWeights);

                if (parentCoil == null)
                {
                    Console.WriteLine($"[Seeder] Could not find an available parent coil for machine '{machine.Name}' to process item '{firstItem.ItemId}'. Skipping remaining items for this machine.");
                    break;
                }

                var parentCoilAvailableWeight = (parentCoil.Snapshot ?? 0m) - allocatedCoilWeights.GetValueOrDefault(parentCoil.Id, 0m);
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
                    TagNumber = parentCoil.TagNumber ?? "FROM-SEEDER",
                    BranchId = branchId,
                    MachineId = machineId,
                    MachineCategory = machine.Category,
                    ParentItemId = parentCoil.ItemId,
                    ParentItemDescription = parentCoil.Description,
                    ParentItemWeight = parentCoil.Snapshot,
                    Instructions = "Seeded work order.",
                    CreatedBy = "SYSTEM",
                    LastUpdatedBy = "SYSTEM",
                    ScheduledStartDate = scheduleStart,
                    Status = WorkOrderStatus.Pending,
                    Priority = WorkOrderPriority.Normal
                };

                decimal currentWoWeight = 0;
                var itemsAddedToThisWo = new List<PickingListItem>();

                foreach (var item in itemsForMachine)
                {
                    var itemWeight = item.Weight ?? 0m;
                    if (itemWeight > 0 && currentWoWeight + itemWeight <= parentCoilAvailableWeight)
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
                    itemsForMachine.RemoveAll(i => itemsAddedToThisWo.Contains(i));
                }
                else
                {
                    Console.WriteLine($"[Seeder] Item '{firstItem.ItemId}' (Weight: {firstItem.Weight}) is too heavy for the largest available parent coil '{parentCoil.ItemId}' (Available: {parentCoilAvailableWeight}). Skipping this item.");
                    itemsForMachine.RemoveAt(0);
                }
            }
        }

        if (newWorkOrders.Any())
        {
            db.WorkOrders.AddRange(newWorkOrders);
            await db.SaveChangesAsync();
        }
    }

    private static async Task<InventoryItem?> FindParentCoilAsync(
        ApplicationDbContext db,
        PickingListItem itemToSchedule,
        MachineCategory category,
        IReadOnlyDictionary<int, decimal> allocatedWeights)
    {
        List<string> parentItemIds;

        if (category == MachineCategory.CTL)
        {
            var relationship = await db.ItemRelationships.AsNoTracking().FirstOrDefaultAsync(ir => ir.ItemCode == itemToSchedule.ItemId);
            if (string.IsNullOrEmpty(relationship?.CoilRelationship)) return null;
            parentItemIds = new List<string> { relationship.CoilRelationship };
        }
        else if (category == MachineCategory.Slitter)
        {
            parentItemIds = new List<string> { itemToSchedule.ItemId };
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
            var allocated = allocatedWeights.GetValueOrDefault(coil.Id, 0m);
            var availableWeight = (coil.Snapshot ?? 0m) - allocated;
            if (availableWeight >= firstItemWeight)
            {
                return coil;
            }
        }

        return null;
    }

    // ---------- helpers ----------

    private static string BuildShipTo(Customer c)
    {
        if (!string.IsNullOrWhiteSpace(c.FullAddress))
            return $"{c.CustomerName}, {c.FullAddress}";
        var parts = new[] { c.Street1, c.Street2, c.City, c.Province, c.PostalCode }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var addr = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(addr) ? c.CustomerName : $"{c.CustomerName}, {addr}";
    }

    private static bool IsStdCoilWidth(decimal w) => w == 36m || w == 48m || w == 60m;

    private static int RouteMachineId(
        decimal? width, bool hasLength,
        List<int> coil, List<int> slitter, List<int> ctl, List<int> sheet,
        Random rng)
    {
        if (hasLength)
        {
            // CTL or SHEET (bias slightly to SHEET)
            if (sheet.Count > 0 && (rng.NextDouble() < 0.6 || ctl.Count == 0))
                return sheet[rng.Next(sheet.Count)];
            if (ctl.Count > 0) return ctl[rng.Next(ctl.Count)];
        }
        else
        {
            if (width.HasValue && IsStdCoilWidth(width.Value) && coil.Count > 0)
                return coil[rng.Next(coil.Count)];
            if (slitter.Count > 0) return slitter[rng.Next(slitter.Count)];
        }
        // Fallback: any machine we can find (keeps seed from failing in sparse DBs)
        var any = coil.Concat(slitter).Concat(ctl).Concat(sheet).ToList();
        if (any.Count > 0)
        {
            return any[rng.Next(any.Count)];
        }

        throw new InvalidOperationException("Could not route to a machine. No machines available for the current branch that match the required categories. Please upload machine data for your branch.");
    }

    private static decimal GuessThickness(string? desc)
    {
        if (Contains(desc, "22 GA")) return 0.0299m;
        if (Contains(desc, "24 GA")) return 0.0239m;
        if (Contains(desc, "26 GA")) return 0.0179m;
        return 0.0239m;
    }

    private static bool Contains(string? s, string token)
        => !string.IsNullOrWhiteSpace(s) && s.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

    private static DateTime BusinessDayMinusOne(DateTime shipDate)
    {
        var d = shipDate.DayOfWeek == DayOfWeek.Monday ? shipDate.AddDays(-3) : shipDate.AddDays(-1);
        if (d.DayOfWeek == DayOfWeek.Saturday) d = d.AddDays(-1);
        if (d.DayOfWeek == DayOfWeek.Sunday) d = d.AddDays(-2);
        return d.Date;
    }

    private static IEnumerable<T> PickRandom<T>(IReadOnlyList<T> src, int count, Random rng)
    {
        if (src.Count <= count) return src.Take(count);
        var seen = new HashSet<int>();
        var list = new List<T>(count);
        while (list.Count < count)
        {
            var i = rng.Next(src.Count);
            if (seen.Add(i)) list.Add(src[i]);
        }
        return list;
    }

    private static string BuildItemDescription(InventoryItem inv, bool hasLen, int machineId)
    {
        // Rough, PDF-like flavor lines
        if (hasLen)
        {
            // SHEET/CTL → include size
            var w = inv.Width ?? 48m;
            var l = inv.Length ?? 120m;
            return $"{(inv.Description ?? inv.ItemId)} — SHEET {w:0.#}\" x {l:0.#}\" (SOURCE: STOCK)";
        }

        if (inv.Width.HasValue)
        {
            return $"{(inv.Description ?? inv.ItemId)} — {(IsStdCoilWidth(inv.Width.Value) ? "COIL slit" : "SLITTER program")} @{inv.Width:0.###}\" (SOURCE: STOCK)";
        }

        return $"{(inv.Description ?? inv.ItemId)} — COIL (SOURCE: STOCK)";
    }
}