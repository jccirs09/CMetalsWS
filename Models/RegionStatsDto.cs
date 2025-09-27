namespace CMetalsWS.Models
{
    public class RegionStatsDto
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;
        public int ActiveOrders { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalWeight { get; set; }
        public int PendingPickups { get; set; }
        public decimal AverageCost { get; set; }
        public string AverageDeliveryTime { get; set; } = string.Empty;
        public int CompletionPercentage { get; set; }
        public string? PrimaryContactName { get; set; }
        public string? PrimaryContactPhone { get; set; }
        public List<string> Alerts { get; set; } = new();
    }
}