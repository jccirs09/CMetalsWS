using System.Collections.Generic;
using System.Linq;
using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public class RouteStopDto
    {
        public int StopNumber { get; set; }
        public string Destination { get; set; } = default!;
        public string WorkOrderNumber { get; set; } = default!;
        public decimal Weight { get; set; }
    }

    public class RouteService
    {
        public IEnumerable<RouteStopDtp> BuildRouteForLoad(Load load)
        {
            return load.Items
                .OrderBy(i => i.Destination)
                .Select((item, index) => new RouteStopDtp
                {
                    StopNumber = index + 1,
                    Destination = item.Destination,
                    PickingListNumber = item.PickingList?.SalesOrderNumber ?? "",
                    Weight = item.Weight
                });
        }
    }
}
