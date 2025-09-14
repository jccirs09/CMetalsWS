using CMetalsWS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IDashboardFeed
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
        Task<List<NowPlayingDto>> GetNowPlayingAsync();
        Task<List<ActivePullingSessionDto>> GetActivePullingAsync();
    }
}
