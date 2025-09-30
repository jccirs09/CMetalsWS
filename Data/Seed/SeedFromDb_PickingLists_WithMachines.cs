using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Data.Seed;

public static class SeedFromDb_PickingLists_WithMachines
{
    private const int PickingListsPerRun = 20;
    private const int MinLines = 2;
    private const int MaxLines = 6;
    private const int ShipHorizonDays = 60;

    /// <summary>
    /// Seeds Picking Lists ONLY. No work orders created here.
    /// </summary>
    public static async Task RunPickingListsOnlyAsync(ApplicationDbContext db, int branchId)
    {
        var rng = new Random();

        // Optional self-heal for CTL mapping
        await EnsureItemRelationshipsExistAsync(db, rng);

        // ----- Load sources from DB -----
        var customers = await db.Set<Customer>().AsNoTracking()
            .Where(c => c.Active && !string.IsNullOrWhiteSpace(c.CustomerName))
            .ToListAsync();
        if (customers.Count == 0) return;

        var regions = await db.Set<DestinationRegion>().AsNoTracking()
            .Select(r => r.Id).ToListAsync();
        if (regions.Count == 0) regions = Enumerable.Range(1, 4).ToList();

        // Machines for routing
        var machines = await db.Set<Machine>().AsNoTracking()
            .Where(m => m.BranchId == branchId).ToListAsync();

        var coilMachines   = machines.Where(m => m.Category == MachineCategory.Coil).Select(m => m.Id).ToList();
        var slitterMachines= machines.Where(m => m.Category == MachineCategory.Slitter).Select(m => m.Id).ToList();
        var ctlMachines    = machines.Where(m => m.Category == MachineCategory.CTL).Select(m => m.Id).ToList();
        var sheetMachines  = machines.Where(m => m.Category == MachineCategory.Sheet).Select(m => m.Id).ToList();

        // Inventory pools
        var ctlRelationships = await db.ItemRelationships.AsNoTracking()
            .Where(ir => !string.IsNullOrEmpty(ir.CoilRelationship))
            .ToListAsync();
        var ctlItemCodes = ctlRelationships.Select(ir => ir.ItemCode).ToHashSet();

        var ctlSourceItems = await db.Set<InventoryItem>().AsNoTracking()
            .Where(i => ctlItemCodes.Contains(i.ItemId)).ToListAsync();

        var slitterSourceItems = await db.Set<InventoryItem>().AsNoTracking()
            .Where(i => i.SnapshotUnit == "LBS" && (!i.Length.HasValue || i.Length == 0)).ToListAsync();

        var sheetSourceItems = await db.Set<InventoryItem>().AsNoTracking()
            .Where(i => i.SnapshotUnit == "PCS" && i.Length.HasValue && i.Length > 0).ToListAsync();

        if (!ctlSourceItems.Any() || !slitterSourceItems.Any())
        {
            Console.WriteLine("[Seeder] Skip: missing CTL or Slitter inventory.");
            return;
        }

        // How many to create
        const int toMake = PickingListsPerRun;
        var today = DateTime.Today;
        var reps   = new[] { "CRAIG MUDIE", "DYLAN WILLIAMS", "SPENCER CHAPMAN", "NICOLE HARRIS", "JACK LAM" };
        var buyers = new[] { "JANE", "STEVE ARMITAGE", "ROBYN LOURENCO", "PROCUREMENT" };

        var existingCount = await db.PickingLists.Where(p => p.BranchId == branchId).CountAsync();
        int soBase = 39250000 + existingCount + rng.Next(1000);
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
            var items = new List<PickingListItem>(lineCount);
            var perSheetMap = new Dictionary<PickingListItem, decimal>();
            decimal totalWeight = 0m;
            int ln = 1;
            var tempPickedItems = new HashSet<string>();

            for (int j = 0; j < lineCount; j++)
            {
                var machineId = RouteMachineId(null, rng.NextDouble() > 0.5, coilMachines, slitterMachines, ctlMachines, sheetMachines, rng);
                var machineCategory = machines.FirstOrDefault(m => m.Id == machineId)?.Category;

                InventoryItem? inv = null;
                if (machineCategory == MachineCategory.CTL && ctlSourceItems.Any())
                    inv = ctlSourceItems[rng.Next(ctlSourceItems.Count)];
                else if (machineCategory == MachineCategory.Slitter && slitterSourceItems.Any())
                    inv = slitterSourceItems[rng.Next(slitterSourceItems.Count)];
                else if (machineCategory == MachineCategory.Sheet && sheetSourceItems.Any())
                    inv = sheetSourceItems[rng.Next(sheetSourceItems.Count)];
                else if (machineCategory == MachineCategory.Coil && slitterSourceItems.Any())
                    inv = slitterSourceItems[rng.Next(slitterSourceItems.Count)];

                if (inv == null || !tempPickedItems.Add(inv.ItemId))
                {
                    var fallbackSource = slitterSourceItems.Concat(ctlSourceItems).ToList();
                    if (!fallbackSource.Any()) continue;
                    inv = fallbackSource[rng.Next(fallbackSource.Count)];
                    if (!tempPickedItems.Add(inv.ItemId)) continue;
                }

                var hasLen = inv.Length.HasValue && inv.Length.Value > 0m;
                var hasWid = inv.Width.HasValue && inv.Width.Value > 0m;
                string unit;
                decimal qty;
                decimal weight;
                decimal perSheet = 0;

                if (hasLen)
                {
                    unit = "PCS";
                    qty = (inv.SnapshotUnit?.Equals("PCS", StringComparison.OrdinalIgnoreCase) == true && inv.Snapshot is > 0)
                        ? Math.Max(1, Math.Round(inv.Snapshot.Value))
                        : rng.Next(10, 151);
                    var thickness = GuessThickness(inv.Description);
                    var density = 0.283m;
                    var w = inv.Width ?? 48m;
                    var l = inv.Length ?? 120m;
                    perSheet = w * l * thickness * density;
                    weight = Math.Max(1m, Math.Round(perSheet * qty, 3));
                }
                else
                {
                    unit = "LBS";
                    if (inv.SnapshotUnit?.Equals("LBS", StringComparison.OrdinalIgnoreCase) == true && inv.Snapshot is > 500)
                    {
                        var baseLbs = inv.Snapshot!.Value;
                        var wiggle = rng.Next(-500, 501);
                        weight = Math.Max(800, Math.Round(baseLbs + wiggle, 0));
                    }
                    else
                    {
                        weight = hasWid && IsStdCoilWidth(inv.Width!.Value) ? rng.Next(3000, 14001) : rng.Next(1500, 9001);
                    }
                    qty = weight;
                }

                var lineStatus =
                    (pl.Status == PickingListStatus.Pending)
                        ? (machineCategory == MachineCategory.Coil || machineCategory == MachineCategory.Sheet
                            ? PickingLineStatus.AssignedPulling
                            : PickingLineStatus.AssignedProduction)
                        : PickingLineStatus.Pending;

                var item = new PickingListItem
                {
                    LineNumber = ln++,
                    ItemId = inv.ItemId,
                    ItemDescription = BuildItemDescription(inv, hasLen),
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
        db.ChangeTracker.Clear();
        Console.WriteLine("[Seeder] Picking Lists seeded (no WorkOrders created).");
    }

    /// <summary>
    /// Separate process: build Work Orders from existing PickingListItems
    /// MachineCategory ∈ {CTL, Slitter} AND Status = AssignedProduction
    /// On success, updates item.Status -> WorkOrder
    /// </summary>
    public static async Task GenerateWorkOrdersFromBacklogAsync(
        ApplicationDbContext db,
        int branchId,
        int targetMaxLbsPerWO = 20000,
        int maxItemsPerWO = 10)
    {
        // Prefetch scheduled PLI ids to avoid duplicates
        var alreadyScheduled = await db.WorkOrderItems
            .AsNoTracking()
            .Where(x => x.PickingListItemId != null)
            .Select(x => x.PickingListItemId!.Value)
            .ToHashSetAsync();

        // Trackable query (we'll update statuses)
        var backlog = await db.PickingListItems
            .Include(pli => pli.PickingList)
            .Include(pli => pli.Machine)
            .Where(pli =>
                pli.PickingList.BranchId == branchId &&
                pli.MachineId.HasValue &&
                pli.Machine != null &&
                (pli.Machine.Category == MachineCategory.CTL || pli.Machine.Category == MachineCategory.Slitter) &&
                pli.Status == PickingLineStatus.AssignedProduction)
            .OrderBy(pli => pli.ScheduledProcessingDate ?? pli.PickingList.ShipDate)
            .ThenBy(pli => pli.PickingList.Priority)
            .ToListAsync();

        // Remove items already scheduled
        backlog = backlog.Where(pli => !alreadyScheduled.Contains(pli.Id)).ToList();

        if (!backlog.Any())
        {
            Console.WriteLine("[WO-Gen] No eligible items found (or all already scheduled).");
            return;
        }

        var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId);
        var branchCode = branch?.Code ?? "00";
        var woCounter = await db.WorkOrders.CountAsync(w => w.BranchId == branchId);

        // Group by machine to keep orders machine-focused
        var byMachine = backlog.GroupBy(i => i.MachineId!.Value);

        using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var newWOs = new List<WorkOrder>();

            foreach (var group in byMachine)
            {
                var machineId = group.Key;
                var machine = group.First().Machine!;

                // Optional: also group by processing date to keep day buckets tidy
                foreach (var dayBucket in group
                             .GroupBy(i => (i.ScheduledProcessingDate ?? i.PickingList.ShipDate).Value.Date)
                             .OrderBy(g => g.Key))
                {
                    var items = dayBucket.ToList();
                    var cursor = 0;

                    while (cursor < items.Count)
                    {
                        var wo = new WorkOrder
                        {
                            WorkOrderNumber = $"W{branchCode}{++woCounter:0000000}",
                            TagNumber = "AUTO-SEED",
                            BranchId = branchId,
                            MachineId = machineId,
                            MachineCategory = machine.Category,
                            Instructions = "Auto-generated from backlog.",
                            CreatedBy = "SYSTEM",
                            LastUpdatedBy = "SYSTEM",
                            ScheduledStartDate = dayBucket.Key.AddHours(8), // 08:00 local
                            Status = WorkOrderStatus.Pending,
                            Priority = WorkOrderPriority.Normal,
                            Items = new List<WorkOrderItem>()
                        };

                        decimal total = 0m;
                        int added = 0;

                        while (cursor < items.Count && added < maxItemsPerWO)
                        {
                            var pli = items[cursor];
                            var wt = pli.Weight ?? 0m;
                            if (wt <= 0) { cursor++; continue; }

                            if (total + wt > targetMaxLbsPerWO) break;

                            wo.Items.Add(new WorkOrderItem
                            {
                                PickingListItemId = pli.Id,
                                ItemCode = pli.ItemId,
                                Description = pli.ItemDescription,
                                SalesOrderNumber = pli.PickingList.SalesOrderNumber,
                                CustomerName = pli.PickingList.SoldTo,
                                OrderQuantity = pli.Quantity,
                                OrderWeight = pli.Weight,
                                Width = pli.Width,
                                Length = pli.Length,
                                Unit = pli.Unit,
                                Status = WorkOrderItemStatus.Pending,
                                IsStockItem = false
                            });

                            // Flip status ONLY when attached to WO
                            pli.Status = PickingLineStatus.WorkOrder;

                            total += wt;
                            added++;
                            cursor++;
                        }

                        if (wo.Items.Count > 0)
                        {
                            wo.DueDate = wo.Items
                                .Select(x => items.First(ii => ii.Id == x.PickingListItemId) // safe; same list
                                    .ScheduledShipDate)
                                .Min();

                            // crude duration: 30 mins per item
                            var estimated = TimeSpan.FromMinutes(30 * wo.Items.Count);
                            wo.ScheduledEndDate = wo.ScheduledStartDate?.Add(estimated);

                            newWOs.Add(wo);
                        }
                        else
                        {
                            // If nothing fit due to caps, bump cursor to avoid infinite loop
                            cursor++;
                        }
                    }
                }
            }

            if (newWOs.Count == 0)
            {
                Console.WriteLine("[WO-Gen] Nothing created (weight caps or constraints prevented batching).");
                await tx.RollbackAsync();
                return;
            }

            db.WorkOrders.AddRange(newWOs);
            var changed = await db.SaveChangesAsync();
            await tx.CommitAsync();

            var itemsCount = newWOs.Sum(w => w.Items.Count);
            Console.WriteLine($"[WO-Gen] Created {newWOs.Count} WOs with {itemsCount} items. DB changes: {changed} rows.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            Console.WriteLine($"[WO-Gen] ERROR: {ex.Message}");
            throw;
        }
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
            if (ctl.Count > 0 && (rng.NextDouble() < 0.9 || sheet.Count == 0))
                return ctl[rng.Next(ctl.Count)];
            if (sheet.Count > 0)
                return sheet[rng.Next(sheet.Count)];
        }
        else
        {
            if (slitter.Count > 0 && (rng.NextDouble() < 0.9 || coil.Count == 0))
                return slitter[rng.Next(slitter.Count)];
            if (coil.Count > 0)
                return coil[rng.Next(coil.Count)];
        }

        var any = coil.Concat(slitter).Concat(ctl).Concat(sheet).ToList();
        if (any.Count > 0) return any[rng.Next(any.Count)];

        throw new InvalidOperationException("No machines available in this branch. Seed machines first.");
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

    private static string NormalizeToBaseCoilId(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return itemId;
        var m = Regex.Match(itemId, @"^(.*?)-\d+(\.\d+)?$");
        return m.Success ? m.Groups[1].Value : itemId;
    }

    private static async Task EnsureItemRelationshipsExistAsync(ApplicationDbContext db, Random rng)
    {
        if (await db.ItemRelationships.AnyAsync()) return;

        var coils = await db.Set<InventoryItem>()
            .AsNoTracking()
            .Where(i => i.SnapshotUnit == "LBS" && (!i.Length.HasValue || i.Length.Value == 0))
            .Select(i => i.ItemId)
            .ToListAsync();

        var sheets = await db.Set<InventoryItem>()
            .AsNoTracking()
            .Where(i => i.SnapshotUnit == "PCS" && i.Length.HasValue && i.Length.Value > 0)
            .ToListAsync();

        if (!coils.Any() || !sheets.Any())
        {
            Console.WriteLine("[Seeder Self-Heal] Missing coils/sheets for ItemRelationships.");
            return;
        }

        var newRelationships = new List<ItemRelationship>();
        foreach (var sheet in sheets)
        {
            newRelationships.Add(new ItemRelationship
            {
                ItemCode = sheet.ItemId,
                Description = sheet.Description ?? string.Empty,
                CoilRelationship = coils[rng.Next(coils.Count)]
            });
        }

        db.ItemRelationships.AddRange(newRelationships);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Console.WriteLine($"[Seeder Self-Heal] Created {newRelationships.Count} ItemRelationships.");
    }

    private static string BuildItemDescription(InventoryItem inv, bool hasLen)
    {
        var desc = string.IsNullOrWhiteSpace(inv.Description) ? inv.ItemId : inv.Description;

        if (hasLen)
        {
            var w = inv.Width ?? 48m;
            var l = inv.Length ?? 120m;
            return $"{desc} — SHEET {w:0.#}\" x {l:0.#}\" (SOURCE: STOCK)";
        }

        if (inv.Width.HasValue)
        {
            var w = inv.Width.Value;
            var flavor = IsStdCoilWidth(w) ? "COIL slit" : "SLITTER program";
            return $"{desc} — {flavor} @{w:0.###}\" (SOURCE: STOCK)";
        }

        return $"{desc} — COIL (SOURCE: STOCK)";
    }
}