using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    public enum RouteStatus
    {
        Planned = 0,
        InProgress = 1,
        Completed = 2,
        Canceled = 3
    }

    public class TruckRoute
    {
        public int Id { get; set; }
        public int BranchId { get; set; }

        [Required]
        public DateTime RouteDate { get; set; } // day this route runs

        // Region or group key taken from Customer.LocationCode
        [MaxLength(32)]
        public string RegionCode { get; set; } = "UNSET";

        public int? TruckId { get; set; }
        public Truck? Truck { get; set; }

        public RouteStatus Status { get; set; } = RouteStatus.Planned;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedUtc { get; set; }

        public ICollection<TruckRouteStop> Stops { get; set; } = new List<TruckRouteStop>();
    }

    public class TruckRouteStop
    {
        public int Id { get; set; }

        public int RouteId { get; set; }
        public TruckRoute? Route { get; set; }

        public int LoadId { get; set; }
        public Load? Load { get; set; }

        public int StopOrder { get; set; }

        public DateTime? PlannedStart { get; set; }
        public DateTime? PlannedEnd { get; set; }

        public DateTime? ActualDepart { get; set; }
        public DateTime? ActualArrive { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }
    }
}
