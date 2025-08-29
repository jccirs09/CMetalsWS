using System.IO;
using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public interface IWorkOrderPdfParser
    {
        WorkOrder Parse(Stream pdfStream, int branchId);
    }
}
