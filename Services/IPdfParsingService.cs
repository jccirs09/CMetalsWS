using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public interface IPdfParsingService
    {
        Task<PickingList> ParseAsync(Stream pdfStream, string sourceFileName);
    }
}
