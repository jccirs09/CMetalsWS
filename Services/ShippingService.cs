using CMetalsWS.Data;
using CMetalsWS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class ShippingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ShippingService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<DeliveryRegion>> GetRegionalDataAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var regions = await db.DestinationRegions
                .AsNoTracking()
                .Include(r => r.Coordinator)
                .ToListAsync();

            var inactiveStatuses = new[] { PickingListStatus.Shipped, PickingListStatus.Cancelled };
            var pendingPickupStatuses = new[] { PickingListStatus.ReadyToShip, PickingListStatus.Ready };

            var pickingListStats = await db.PickingLists
                .AsNoTracking()
                .Where(p => p.DestinationRegionId != null && !inactiveStatuses.Contains(p.Status))
                .GroupBy(p => p.DestinationRegionId)
                .Select(g => new
                {
                    RegionId = g.Key,
                    ActiveOrders = g.Count(),
                    PendingPickups = g.Count(p => pendingPickupStatuses.Contains(p.Status))
                })
                .ToListAsync();

            var statsLookup = pickingListStats.ToDictionary(s => s.RegionId);

            return regions.Select(r =>
            {
                var metrics = new RegionMetrics();
                if (statsLookup.TryGetValue(r.Id, out var stats))
                {
                    metrics.ActiveOrders = stats.ActiveOrders;
                    metrics.PendingPickups = stats.PendingPickups;
                }

                return new DeliveryRegion
                {
                    Id = r.Id.ToString(),
                    Name = r.Name,
                    Type = r.Type,
                    Description = r.Description,
                    Characteristics = new RegionCharacteristics
                    {
                        RequiresPooling = r.RequiresPooling
                    },
                    OperationalInfo = new RegionOperationalInfo
                    {
                        Coordinator = r.Coordinator?.FullName ?? "Unassigned",
                        Phone = r.Coordinator?.PhoneNumber ?? "N/A"
                    },
                    Metrics = metrics
                };
            }).ToList();
        }

        public async Task<List<ShippingLoad>> GetLoadsAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var loads = await db.Loads
                .AsNoTracking()
                .Include(l => l.Truck)
                    .ThenInclude(t => t.Driver)
                .Include(l => l.Items)
                    .ThenInclude(i => i.PickingList)
                        .ThenInclude(pl => pl.Customer)
                .OrderByDescending(l => l.ShippingDate)
                .Take(50) // Limiting for performance
                .ToListAsync();

            return loads.Select(l => new ShippingLoad
            {
                Id = l.LoadNumber,
                TruckNumber = l.Truck?.Identifier ?? "N/A",
                DriverName = l.Truck?.Driver?.FullName ?? "Unassigned",
                DriverPhone = l.Truck?.Driver?.PhoneNumber ?? "N/A",
                TrailerType = l.Truck?.Description ?? "N/A",
                MaxWeight = (int)(l.Truck?.CapacityWeight ?? 0),
                Status = l.Status.ToString().ToLower(),
                Priority = "normal", // Placeholder, missing in DB model
                ScheduledPickup = l.ShippingDate ?? DateTime.Now,
                EstimatedDelivery = l.ShippingDate?.AddHours(8), // Placeholder
                CurrentWeight = (int)l.TotalWeight,
                UtilizationPercentage = (l.Truck?.CapacityWeight > 0) ? (int)(l.TotalWeight / l.Truck.CapacityWeight * 100) : 0,
                Notes = l.Notes,
                CreatedAt = l.ShippingDate ?? DateTime.Now, // Placeholder
                CreatedBy = "System", // Placeholder
                DeliveryRegion = l.Items.FirstOrDefault()?.PickingList?.Customer?.DestinationRegion?.Name ?? "Unknown",
                Items = l.Items.Select(i => new CMetalsWS.Models.LoadItem
                {
                    Id = i.Id.ToString(),
                    CustomerName = i.PickingList.Customer?.CustomerName ?? "N/A",
                    OrderNumber = i.PickingList.SalesOrderNumber,
                    Destination = i.PickingList.Customer?.FullAddress ?? i.PickingList.ShipTo ?? "N/A",
                    Weight = (int)i.ShippedWeight,
                    Pieces = (int)i.ShippedQuantity,
                    SpecialInstructions = "" // Placeholder
                }).ToList()
            }).ToList();
        }

        public async Task<List<AvailableOrder>> GetAvailableOrdersAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var shippableStatuses = new[] { PickingListStatus.Pending, PickingListStatus.InProgress, PickingListStatus.ReadyToShip };

            var orders = await db.PickingLists
                .AsNoTracking()
                .Where(pl => shippableStatuses.Contains(pl.Status) && pl.TotalWeight > 0)
                .Include(pl => pl.Customer)
                .Include(pl => pl.DestinationRegion)
                .Include(pl => pl.Items)
                .OrderByDescending(pl => pl.ShipDate)
                .ToListAsync();

            return orders.Select(o => new AvailableOrder
            {
                Id = o.Id.ToString(),
                CustomerName = o.Customer?.CustomerName ?? o.SoldTo ?? "N/A",
                OrderNumber = o.SalesOrderNumber,
                Destination = o.Customer?.FullAddress ?? o.ShipTo ?? "N/A",
                Weight = (int)o.TotalWeight,
                Pieces = o.Items.Count, // This is not accurate, but a placeholder
                Priority = o.Priority < 50 ? "high" : "normal", // Placeholder logic
                ReadyDate = o.ShipDate ?? DateTime.Today,
                DeliveryRegion = o.DestinationRegion?.Name ?? "Unknown",
                Distance = 0, // Placeholder
                PickingStatus = o.Status.ToString(),
                Items = o.Items.Select(i => new OrderItem
                {
                    Id = i.Id.ToString(),
                    Description = i.ItemDescription,
                    Quantity = (int)i.Quantity,
                    Weight = (int)(i.Weight ?? 0),
                    Status = i.Status.ToString()
                }).ToList()
            }).ToList();
        }

        public async Task<List<TruckInfo>> GetAvailableTrucksAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var activeLoadStatuses = new[] { LoadStatus.Pending, LoadStatus.Loaded, LoadStatus.Scheduled, LoadStatus.InTransit };

            var truckLoads = await db.Loads
                .AsNoTracking()
                .Where(l => l.TruckId != null && activeLoadStatuses.Contains(l.Status))
                .GroupBy(l => l.TruckId)
                .Select(g => new
                {
                    TruckId = g.Key,
                    CurrentWeight = g.Sum(l => l.TotalWeight)
                })
                .ToDictionaryAsync(x => x.TruckId!.Value, x => x.CurrentWeight);

            var trucks = await db.Trucks
                .AsNoTracking()
                .Where(t => t.IsActive)
                .Include(t => t.Driver)
                .ToListAsync();

            return trucks.Select(t => new TruckInfo
            {
                Id = t.Id.ToString(),
                Name = t.Name,
                Capacity = (int)t.CapacityWeight,
                CurrentLoad = truckLoads.TryGetValue(t.Id, out var weight) ? (int)weight : 0,
                IsRecommended = false, // Placeholder
                Type = t.Description ?? "N/A",
                Driver = t.Driver?.FullName ?? "Unassigned",
                BranchId = t.BranchId
            }).ToList();
        }

        public async Task<List<RegionalAlert>> GetRegionalAlertsAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var alerts = new List<RegionalAlert>();
            var shippableStatuses = new[] { PickingListStatus.ReadyToShip, PickingListStatus.Ready };

            var poolingRegions = await db.DestinationRegions
                .AsNoTracking()
                .Where(r => r.RequiresPooling)
                .ToListAsync();

            foreach (var region in poolingRegions)
            {
                var readyOrderCount = await db.PickingLists
                    .AsNoTracking()
                    .CountAsync(pl => pl.DestinationRegionId == region.Id && shippableStatuses.Contains(pl.Status));

                if (readyOrderCount > 0)
                {
                    alerts.Add(new RegionalAlert
                    {
                        Title = $"{region.Name} Pool Alert",
                        Content = $"{readyOrderCount} order(s) ready for consolidation and shipping.",
                        Color = "info"
                    });
                }
            }

            return alerts;
        }
    }
}