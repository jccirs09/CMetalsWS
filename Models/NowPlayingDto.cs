namespace CMetalsWS.Models
{
    public class NowPlayingDto
    {
        public string SalesOrderNumber { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string MachineName { get; set; } = default!;
        public string CustomerName { get; set; } = default!;
        public int LineItems { get; set; }
        public decimal TotalWeight { get; set; }
        public string OperatorName { get; set; } = default!;
        public int Progress { get; set; }
    }
}
