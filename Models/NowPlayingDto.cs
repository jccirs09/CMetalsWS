namespace CMetalsWS.Models
{
    public class NowPlayingDto
    {
        public string SalesOrderNumber { get; set; }
        public string Status { get; set; }
        public string MachineName { get; set; }
        public string CustomerName { get; set; }
        public int LineItems { get; set; }
        public decimal TotalWeight { get; set; }
        public string OperatorName { get; set; }
        public int Progress { get; set; }
    }
}
