using System.ComponentModel.DataAnnotations;
namespace CMetalsWS.Data
{
    public enum LoadStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    public class Load
    {
        public int Id { get; set; }
        public string LoadNumber { get; set; } = default!;
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }
        public int TruckId { get; set; }
        public Truck? Truck { get; set; }
        public DateTime ScheduledDate { get; set; }
        public LoadStatus Status { get; set; } = LoadStatus.Pending;
        public ICollection<LoadItem> Items { get; set; } = new List<LoadItem>();
    }

    public class LoadItem
    {
        public int Id { get; set; }
        public int LoadId { get; set; }
        public Load? Load { get; set; }
        public int WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }
        public decimal Weight { get; set; }
        public string Destination { get; set; } = default!;
    }
}


        
