using System.Threading;
using System.Threading.Tasks;
using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public interface IPickingListStatusUpdater
    {
        Task UpdatePickingListStatusAsync(int pickingListId, PickingListStatus status, CancellationToken ct = default);
    }
}
