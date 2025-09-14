using System.Threading.Tasks;

namespace CMetalsWS.Hubs
{
    /// <summary>
    /// Defines the client-side methods that the DashboardHub can invoke.
    /// </summary>
    public interface IDashboardClient
    {
        /// <summary>
        /// Notifies the client that the dashboard data has changed and should be refreshed.
        /// </summary>
        Task RefreshDashboard();
    }
}
