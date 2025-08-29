using System.IO;
using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public interface IPickingListPdfParser
    {
        PickingList Parse(Stream pdfStream, int branchId, int? customerId = null, int? truckId = null);
    }
}
