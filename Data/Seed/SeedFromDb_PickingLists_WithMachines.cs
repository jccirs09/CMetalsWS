using System.Globalization;
using System.Text.RegularExpressions;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Data.Seed;

public static class SeedFromDb_PickingLists_WithMachines
{
    private const int PickingListsPerRun = 20;
    private const int MinLines = 2;
    private const int MaxLines = 6;
    private const int ShipHorizonDays = 60;

    public static async Task RunAsync(ApplicationDbContext db, int branchId)
    {
        var rng = new Random();

        // ----- Self-healing: Ensure ItemRelationships exist for CTL logic -----
        await EnsureItemRelationshipsExistAsync(db, rng);

        // ----- Load sources from DB -----
        var customers = await db.Set<Customer>().AsNoTracking()
            .Where(c => c.Active && !string.IsNullOrWhiteSpace(c.CustomerName))
            .ToListAsync();
        if (customers.Count == 0) return;

        var regions = await db.Set<DestinationRegion>().AsNoTracking()
            .Select(r => r.Id).ToListAsync();
        if (regions.Count == 0) regions = Enumerable.Range(1, 4).ToList();

        // ----- Load machine lists -----
        var machines = await db.Set<Machine>().AsNoTracking()
            .Where(m => m.BranchId == branchId).ToListAsync();

        var coilMachines = machines.Where(m => m.Category == MachineCategory.Coil).Select(m => m.Id).ToList();
        var slitterMachines = machines.Where(m => m.Category == MachineCategory.Slitter).Select(m => m.Id).ToList();
        var ctlMachines = machines.Where(m => m.Category == MachineCategory.CTL).Select(m => m.Id).ToList();
        var sheetMachines = machines.Where(m => m.Category == MachineCategory.Sheet).Select(m => m.Id).ToList();

        // ----- Load valid inventory sources for each machine type -----
        var ctlRelationships = await db.ItemRelationships.AsNoTracking()
            .Where(ir => !string.IsNullOrEmpty(ir.CoilRelationship)).ToListAsync();
        var ctlItemCodes = ctlRelationships.Select(ir => ir.ItemCode).ToHashSet();

        var ctlSourceItems = await db.Set<InventoryItem>().AsNoTracking()
            .Where(i => ctlItemCodes.Contains(i.ItemId)).ToListAsync();

        var slitterSourceItems = await db.Set<InventoryItem>().AsNoTracking()
            .Where(i => i.SnapshotUnit == "LBS" && (!i.Length.HasValue || i.Length == 0))
            .ToListAsync();

        var sheetSourceItems = await db.Set<InventoryItem>().AsNoTracking()
            .Where(i => i.SnapshotUnit == "PCS" && i.Length.HasValue && i.Length > 0)
            .ToListAsync();

        if (!ctlSourceItems.Any() || !slitterSourceItems.Any())
        {
            Console.WriteLine("[Seeder] Could not run. Missing valid inventory for CTL or Slitter machines.");
            return;
        }

        // ----- Figure out how many to create -----
        const int toMake = PickingListsPerRun;
        var today = DateTime.Today;
        var reps = new[] { "CRAIG MUDIE", "DYLAN WILLIAMS", "SPENCER CHAPMAN", "NICOLE HARRIS", "JACK LAM" };
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
        // The Work Order seeder specifically looks for CTL and Slitter items.
        // We must ensure that a high percentage of items are routed to these machines.
        if (hasLength)
        {
            // Has length -> CTL or Sheet. Prioritize CTL heavily.
            if (ctl.Count > 0 && (rng.NextDouble() < 0.9 || sheet.Count == 0)) // 90% chance for CTL
                return ctl[rng.Next(ctl.Count)];
            if (sheet.Count > 0)
                return sheet[rng.Next(sheet.Count)];
        }
        else
        {
            // No length -> Slitter or Coil. Prioritize Slitter heavily.
            if (slitter.Count > 0 && (rng.NextDouble() < 0.9 || coil.Count == 0)) // 90% chance for Slitter
                return slitter[rng.Next(slitter.Count)];
            if (coil.Count > 0)
                return coil[rng.Next(coil.Count)];
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

    private static bool Contains(string? s, string token)
        => !string.IsNullOrWhiteSpace(s) && s.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

    private static DateTime BusinessDayMinusOne(DateTime shipDate)
    {
        var d = shipDate.DayOfWeek == DayOfWeek.Monday ? shipDate.AddDays(-3) : shipDate.AddDays(-1);
        if (d.DayOfWeek == DayOfWeek.Saturday) d = d.AddDays(-1);
        if (d.DayOfWeek == DayOfWeek.Sunday) d = d.AddDays(-2);
        return d.Date;
    }

    private static async Task EnsureItemRelationshipsExistAsync(ApplicationDbContext db, Random rng)
    {
        if (await db.ItemRelationships.AnyAsync())
        {
            return; // Relationships already exist, no need to seed.
        }

        var coils = await db.InventoryItems
            .AsNoTracking()
            .Where(i => i.SnapshotUnit == "LBS" && (!i.Length.HasValue || i.Length.Value == 0))
            .Select(i => i.ItemId)
            .ToListAsync();

        var sheets = await db.InventoryItems
            .AsNoTracking()
            .Where(i => i.SnapshotUnit == "PCS" && i.Length.HasValue && i.Length.Value > 0)
            .ToListAsync();

        if (!coils.Any() || !sheets.Any())
        {
            Console.WriteLine("[Seeder Self-Heal] Could not create ItemRelationships. Missing source coils or sheets in Inventory.");
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
        db.ChangeTracker.Clear(); // Clear tracking after seeding relationships
        Console.WriteLine($"[Seeder Self-Heal] Created {newRelationships.Count} new ItemRelationships.");
    }
}