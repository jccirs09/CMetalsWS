using System.IO;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IPickingListIngestor
    {
        Task<int> UploadAsync(Stream pdf, string fileName, int branchId, string uploadedBy);
    }
}
