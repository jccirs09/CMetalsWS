namespace CMetalsWS.Data
{
    /// <summary>Represents a truck used for deliveries.</summary>
    public class Truck
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = default!; // e.g. license plate or fleet number
        public decimal CapacityWeight { get; set; }
        public decimal CapacityVolume { get; set; }
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        // Optional: assign picking lists to this truck
        public ICollection<PickingList> AssignedPickings { get; set; } = new List<PickingList>();
    }
}
