using CMetalsWS.Data;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IPickingListDraftStore
    {
        Task<Guid> CreateAsync(Stream pdf, string fileName, int branchId);
        Task<PickingList?> GetAsync(Guid id);
        Task<bool> ReplaceAsync(Guid id, PickingList draft);
        void Remove(Guid id);
    }
}
