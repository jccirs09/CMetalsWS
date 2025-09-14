using Microsoft.AspNetCore.SignalR;

namespace CMetalsWS.Hubs
{
    public class DashboardHub : Hub<IDashboardClient>
    {
        // This hub doesn't need any server-callable methods for now.
        // Its purpose is to allow the server to invoke methods on clients.
        // We can add methods here later if clients need to send messages to the server.
    }
}
