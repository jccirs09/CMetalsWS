namespace CMetalsWS.Data
{
    public class RouteStopDtp
    {
        public int StopNumber { get; set; }
        public string Destination { get; set; } = default!;
        public string PickingListNumber { get; set; } = default!;
        public decimal Weight { get; set; }
    }
}
