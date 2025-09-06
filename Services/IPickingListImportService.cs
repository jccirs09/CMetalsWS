using CMetalsWS.Data;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IPickingListImportService
    {
        Task<PickingListImport> CreateImportAsync(int branchId, string sourcePdfPath, string imagesPath, string modelUsed);
        Task UpdateImportSuccessAsync(int importId, int pickingListId, string rawJson);
        Task UpdateImportFailedAsync(int importId, string error, string? rawJson = null);
        Task<PickingListImport?> GetLatestImportByPickingListIdAsync(int pickingListId);
    }
}
