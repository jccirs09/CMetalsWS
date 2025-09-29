using System;
using System.Collections.Generic;
using System.Linq;
using CMetalsWS.Models;

namespace CMetalsWS.Services
{
    public class ShippingService
    {
        public List<DeliveryRegion> GetRegionalData()
        {
            return new List<DeliveryRegion>
            {
                new() {
                    Id = "local", Name = "Local Delivery", Type = "local", Description = "Same-day and next-day deliveries within metro area",
                    Characteristics = new RegionCharacteristics { FerryDependent = false, RequiresPooling = false },
                    OperationalInfo = new RegionOperationalInfo { Coordinator = "Sarah Chen", Phone = "(604) 555-0123" },
                    Metrics = new RegionMetrics { ActiveOrders = 23, AverageDeliveryTime = "4-8 hours", PendingPickups = 0, UtilizationRate = 85 }
                },
                new() {
                    Id = "out-of-town", Name = "Multi Out of Town Lanes", Type = "out-of-town", Description = "Regional deliveries to multiple towns and cities",
                    Characteristics = new RegionCharacteristics { FerryDependent = false, RequiresPooling = true },
                    OperationalInfo = new RegionOperationalInfo { Coordinator = "Mike Rodriguez", Phone = "(604) 555-0456" },
                    Metrics = new RegionMetrics { ActiveOrders = 18, AverageDeliveryTime = "2-4 days", PendingPickups = 3, UtilizationRate = 72 }
                },
                new() {
                    Id = "island-pool", Name = "Island Pool Trucks", Type = "island-pool", Description = "Consolidated ferry-dependent deliveries to Vancouver Island",
                    Characteristics = new RegionCharacteristics { FerryDependent = true, RequiresPooling = true },
                    OperationalInfo = new RegionOperationalInfo { Coordinator = "Jennifer Wilson", Phone = "(604) 555-0789" },
                    Metrics = new RegionMetrics { ActiveOrders = 12, AverageDeliveryTime = "3-5 days", PendingPickups = 8, UtilizationRate = 68 }
                },
                new() {
                    Id = "okanagan-pool", Name = "Okanagan Pool Trucks", Type = "okanagan-pool", Description = "Pooled deliveries to Okanagan Valley region",
                    Characteristics = new RegionCharacteristics { FerryDependent = false, RequiresPooling = true },
                    OperationalInfo = new RegionOperationalInfo { Coordinator = "Carlos Martinez", Phone = "(604) 555-0321" },
                    Metrics = new RegionMetrics { ActiveOrders = 8, AverageDeliveryTime = "4-7 days", PendingPickups = 5, UtilizationRate = 58 }
                },
                new() {
                    Id = "customer-pickup", Name = "Customer Pickup", Type = "customer-pickup", Description = "Customer self-pickup coordination and scheduling",
                    Characteristics = new RegionCharacteristics { FerryDependent = false, RequiresPooling = false },
                    OperationalInfo = new RegionOperationalInfo { Coordinator = "Lisa Thompson", Phone = "(604) 555-0654" },
                    Metrics = new RegionMetrics { ActiveOrders = 15, AverageDeliveryTime = "Same day", PendingPickups = 9, UtilizationRate = 95 }
                }
            };
        }

        public List<ShippingLoad> GetSampleLoads()
        {
            return new List<ShippingLoad>
            {
                new() {
                    Id = "LD-2024-0001", TruckNumber = "T-401", DriverName = "Mike Rodriguez", DriverPhone = "(604) 555-1234", TrailerType = "flatbed", MaxWeight = 48000, Status = "in-transit", Priority = "high",
                    ScheduledPickup = DateTime.Parse("2024-01-22T08:00:00"), EstimatedDelivery = DateTime.Parse("2024-01-22T16:30:00"),
                    Items = new List<LoadItem> {
                        new() { Id = "LI-001-001", CustomerName = "Pacific Metal Roofing Ltd", OrderNumber = "PMR-8901", Destination = "1245 Industrial Way, Vancouver, BC V5L 3C2", Weight = 12400, Pieces = 48, SpecialInstructions = "Forklift available on site for unloading" },
                        new() { Id = "LI-001-002", CustomerName = "Burnaby Metal Crafters", OrderNumber = "BMC-7788", Destination = "4567 Hastings St, Burnaby, BC V5C 2K8", Weight = 15600, Pieces = 38, SpecialInstructions = "Overhead crane available" }
                    },
                    CurrentWeight = 28000, UtilizationPercentage = 83, Notes = "Local delivery route - both customers have forklift available", CreatedBy = "Sarah Chen", CreatedAt = DateTime.Parse("2024-01-21T14:30:00Z"), DeliveryRegion = "local"
                },
                new() {
                    Id = "LD-2024-0002", TruckNumber = "T-312", DriverName = "Jennifer Wilson", DriverPhone = "(250) 555-2345", TrailerType = "flatbed", MaxWeight = 52000, Status = "dispatched", Priority = "high",
                    ScheduledPickup = DateTime.Parse("2024-01-28T06:00:00"), EstimatedDelivery = DateTime.Parse("2024-01-28T18:00:00"),
                    Items = new List<LoadItem> {
                        new() { Id = "LI-002-001", CustomerName = "Victoria Island Metals", OrderNumber = "VIM-6677", Destination = "1890 Douglas St, Victoria, BC V8T 4K7", Weight = 18200, Pieces = 42, SpecialInstructions = "Ferry booking required - Tsawwassen to Swartz Bay" },
                        new() { Id = "LI-002-002", CustomerName = "Duncan Roofing Supply", OrderNumber = "DRS-9900", Destination = "567 Trans-Canada Hwy, Duncan, BC V9L 3R5", Weight = 21800, Pieces = 65, SpecialInstructions = "Rush order - ferry priority booking" }
                    },
                    CurrentWeight = 40000, UtilizationPercentage = 77, Notes = "Island pool delivery - ferry booked on Tsawwassen-Swartz Bay", CreatedBy = "Jennifer Wilson", CreatedAt = DateTime.Parse("2024-01-27T15:00:00Z"), DeliveryRegion = "island-pool"
                }
            };
        }

        public List<AvailableOrder> GetAvailableOrders()
        {
            return new List<AvailableOrder>
            {
                new() {
                    Id = "ORD-2024-0001", CustomerName = "Pacific Metal Roofing Ltd", OrderNumber = "PMR-8901", Destination = "1245 Industrial Way, Vancouver, BC V5L 3C2", Weight = 12400, Pieces = 48, Priority = "high", ReadyDate = DateTime.Parse("2024-01-20"), DeliveryRegion = "local", Distance = 15, PickingStatus = "ready",
                    Items = new List<OrderItem> {
                        new() { Id = "ITEM-001", Description = "Galvanized Steel Panels", Quantity = 20, Weight = 4800, Status = "ready" },
                        new() { Id = "ITEM-002", Description = "Aluminum Frame Stock", Quantity = 28, Weight = 7600, Status = "picked" }
                    }
                },
                new() {
                    Id = "ORD-2024-0002", CustomerName = "West Side Windows & Doors", OrderNumber = "WSW-4567", Destination = "789 Broadway Ave, Vancouver, BC V5Z 1K5", Weight = 8900, Pieces = 32, Priority = "urgent", ReadyDate = DateTime.Parse("2024-01-20"), DeliveryRegion = "local", Distance = 12, PickingStatus = "ready",
                    Items = new List<OrderItem> {
                        new() { Id = "ITEM-003", Description = "Window Frames", Quantity = 32, Weight = 8900, Status = "packed" }
                    }
                },
                new() {
                    Id = "ORD-2024-0009", CustomerName = "Victoria Island Metals", OrderNumber = "VIM-6677", Destination = "1890 Douglas St, Victoria, BC V8T 4K7", Weight = 18200, Pieces = 42, Priority = "high", ReadyDate = DateTime.Parse("2024-01-21"), DeliveryRegion = "island-pool", Distance = 115, PickingStatus = "ready",
                    Items = new List<OrderItem> {
                        new() { Id = "ITEM-004", Description = "Steel Beams", Quantity = 10, Weight = 12000, Status = "ready" },
                        new() { Id = "ITEM-005", Description = "Rebar", Quantity = 32, Weight = 6200, Status = "ready" }
                    }
                }
            };
        }

        public List<TruckInfo> GetAvailableTrucks()
        {
            return new List<TruckInfo>
            {
                new() { Id = "T-401", Name = "T-401", Capacity = 48000, CurrentLoad = 21300, IsRecommended = true, Type = "Flatbed", Driver = "Shawn Mike" },
                new() { Id = "T-205", Name = "T-205", Capacity = 44000, CurrentLoad = 0, IsRecommended = false, Type = "Enclosed", Driver = "Steve" },
                new() { Id = "T-156", Name = "T-156", Capacity = 42000, CurrentLoad = 0, IsRecommended = false, Type = "Dry-Van", Driver = "John" }
            };
        }

        public List<RegionalAlert> GetRegionalAlerts()
        {
            return new List<RegionalAlert>
            {
                new() { Title = "Island Pool Alert", Content = "Ferry booking required for 8 orders by 4 PM today", Color = "warning" },
                new() { Title = "Okanagan Pool Ready", Content = "Pool consolidation complete - ready for dispatch", Color = "info" },
                new() { Title = "Customer Pickup", Content = "9 orders scheduled for pickup today", Color = "success" }
            };
        }
    }
}