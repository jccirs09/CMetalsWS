namespace CMetalsWS.Services;

public interface IPdfParsingService
{
    Task<string> ParsePdfAsync(Stream pdfStream);
}
