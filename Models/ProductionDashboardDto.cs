namespace CMetalsWS.Models
{
    public class ProductionDashboardDto
    {
        public int Current { get; set; }
        public int Pending { get; set; }
        public int Completed { get; set; }
        public int Awaiting { get; set; }
    }
}