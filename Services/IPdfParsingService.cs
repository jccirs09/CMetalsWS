using CMetalsWS.Services.Dto;

namespace CMetalsWS.Services;

public interface IPdfParsingService
{
    Task<PickingListExtraction?> ParsePdfAsync(Stream pdfStream);
}
