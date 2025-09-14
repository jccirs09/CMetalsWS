using System.IO;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IPickingListIngestor
    {
        Task<CMetalsWS.Models.IngestResult> UploadAsync(Stream pdf, string fileName, int branchId, string uploadedBy);
    }
}
