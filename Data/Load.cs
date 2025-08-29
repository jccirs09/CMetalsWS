using System;
using System.Collections.Generic;

namespace CMetalsWS.Data
{
    public enum LoadStatus
    {
        Pending,
        Loaded,
        Scheduled,
        InTransit,
        Delivered,
        Canceled
    }

    public class Load
    {
        public int Id { get; set; }
        public string LoadNumber { get; set; } = default!;
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }
        public int TruckId { get; set; }
        public Truck? Truck { get; set; }

        // Non-nullable scheduling fields (use default(DateTime) to mean "unset")
        public DateTime? ScheduledDate { get; set; }
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }

        public DateTime? ReadyDate { get; set; }

        public LoadStatus Status { get; set; } = LoadStatus.Pending;
        public ICollection<LoadItem> Items { get; set; } = new List<LoadItem>();
    }

    public class LoadItem
    {
        public int Id { get; set; }
        public int LoadId { get; set; }
        public Load? Load { get; set; }

        public int PickingListId { get; set; }
        public PickingList? PickingList { get; set; }

        public decimal Weight { get; set; }
        public string Destination { get; set; } = default!;
    }
}
