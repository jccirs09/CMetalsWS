using CMetalsWS.Data;

public class RouteService
{
    public IEnumerable<RouteStopDto> BuildRouteForLoad(Load load)
    {
        // Sort load items by destination (this is simplistic; integrate real geo API as needed)
        return load.Items
            .OrderBy(i => i.Destination)
            .Select((item, index) => new RouteStopDto
            {
                StopNumber = index + 1,
                Destination = item.Destination,
                WorkOrderNumber = item.WorkOrder?.WorkOrderNumber ?? "",
                Weight = item.Weight
            });
    }
}