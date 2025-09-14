namespace CMetalsWS.Models
{
    public class DashboardSummaryDto
    {
        public decimal CtlLbsPerHour { get; set; }
        public decimal SlitterLbsPerHour { get; set; }

        public bool IsCtlRunning { get; set; }
        public bool IsSlitterInSetup { get; set; }

        public decimal? PullingLbsPerHour { get; set; }
        public int? ActivePullingSessions { get; set; }

        public int? LoadsToday { get; set; }
        public int? LoadsInTransit { get; set; }
    }
}
