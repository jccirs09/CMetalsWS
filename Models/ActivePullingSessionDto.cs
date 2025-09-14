namespace CMetalsWS.Models
{
    public class ActivePullingSessionDto
    {
        public string OperatorName { get; set; } = string.Empty;
        public string OperatorInitials { get; set; } = string.Empty;
        public string StationInfo { get; set; } = string.Empty; // e.g. "Station 2 - Midwest Region"
        public int CompletedOrders { get; set; }
        public int TotalOrders { get; set; }
        public decimal WeightLbs { get; set; }
        public string SessionTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Active" / "Break"
    }
}
