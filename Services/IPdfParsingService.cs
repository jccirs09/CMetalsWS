using CMetalsWS.Services.Dto;
using System.IO;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IPdfParsingService
    {
        Task<PickingListExtraction?> ParsePdfAsync(Stream pdfStream);
    }
}
