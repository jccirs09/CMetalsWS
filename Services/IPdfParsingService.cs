using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public interface IPdfParsingService
    {
        Task<List<string>> ConvertPdfToImagesAsync(string sourcePdfPath, Guid importGuid);
        Task<(PickingList, List<PickingListItem>)> ParsePickingListAsync(IEnumerable<string> imagePaths);
    }
}
