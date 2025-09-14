using System.Threading;
using System.Threading.Tasks;
using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public sealed class PickingListStatusUpdaterAdapter : IPickingListStatusUpdater
    {
        private readonly PickingListService _inner;

        public PickingListStatusUpdaterAdapter(PickingListService inner)
        {
            _inner = inner;
        }

        public Task UpdatePickingListStatusAsync(int pickingListId, PickingListStatus status, CancellationToken ct = default)
            => _inner.UpdatePickingListStatusAsync(pickingListId, status, ct);
    }
}
