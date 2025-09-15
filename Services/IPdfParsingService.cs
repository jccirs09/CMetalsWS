using CMetalsWS.Data;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;

namespace CMetalsWS.Services
{
    public interface IPdfParsingService
    {
        Task<(PickingList, List<PickingListItem>)> ParsePickingListAsync(byte[] pdfBytes);
    }
}
