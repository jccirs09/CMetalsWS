

namespace CMetalsWS.Data
{
    /// <summary>Represents a warehouse or production site.</summary>
    public class Branch
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        // Navigation collections
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Machine> Machines { get; set; } = new List<Machine>();
        public ICollection<Truck> Trucks { get; set; } = new List<Truck>();
        public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
        public ICollection<PickingList> PickingLists { get; set; } = new List<PickingList>();
    }
}
