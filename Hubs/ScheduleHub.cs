using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CMetalsWS.Hubs
{
    public class ScheduleHub : Hub
    {
        public async Task NotifyWorkOrderUpdated(int workOrderId)
        {
            await Clients.All.SendAsync("WorkOrderUpdated", workOrderId);
        }
    }
}
