using System;
using System.Collections.Generic;

namespace CMetalsWS.Models
{
    public class DeliveryRegion
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public RegionCharacteristics Characteristics { get; set; }
        public RegionOperationalInfo OperationalInfo { get; set; }
        public RegionMetrics Metrics { get; set; }
    }

    public class RegionCharacteristics
    {
        public bool FerryDependent { get; set; }
        public bool RequiresPooling { get; set; }
        public bool IsCustomerPickup { get; set; }
    }

    public class RegionOperationalInfo
    {
        public string Coordinator { get; set; }
        public string Phone { get; set; }
    }

    public class RegionMetrics
    {
        public int ActiveOrders { get; set; }
        public string AverageDeliveryTime { get; set; }
        public int PendingPickups { get; set; }
        public int UtilizationRate { get; set; }
    }

    public class ShippingLoad
    {
        public string Id { get; set; }
        public string TruckNumber { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }
        public string TrailerType { get; set; }
        public int MaxWeight { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime ScheduledPickup { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public List<LoadItem> Items { get; set; }
        public int CurrentWeight { get; set; }
        public int UtilizationPercentage { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DeliveryRegion { get; set; }
    }

    public class LoadItem
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string OrderNumber { get; set; }
        public string Destination { get; set; }
        public int Weight { get; set; }
        public int Pieces { get; set; }
        public string SpecialInstructions { get; set; }
    }

    public class AvailableOrder
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string OrderNumber { get; set; }
        public string Destination { get; set; }
        public int Weight { get; set; }
        public int Pieces { get; set; }
        public string Priority { get; set; }
        public DateTime ReadyDate { get; set; }
        public string DeliveryRegion { get; set; }
        public int Distance { get; set; }
        public string PickingStatus { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem : IEquatable<OrderItem>
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Weight { get; set; }
        public string Status { get; set; }

        public bool Equals(OrderItem? other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }

        public override bool Equals(object? obj) => Equals(obj as OrderItem);

        public override int GetHashCode() => Id.GetHashCode();
    }


    public class TruckInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public int CurrentLoad { get; set; }
        public bool IsRecommended { get; set; }
        public string Type { get; set; }
        public string Driver { get; set; }
        public int BranchId { get; set; }
    }

    public class RegionalAlert
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Color { get; set; }
    }
}