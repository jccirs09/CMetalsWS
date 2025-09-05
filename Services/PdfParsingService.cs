using System.Text;
using UglyToad.PdfPig;

namespace CMetalsWS.Services
{
    public class PdfParsingService : IPdfParsingService
    {
        public Task<string> ParsePdfAsync(Stream pdfStream)
        {
            var sb = new StringBuilder();
            using (var document = PdfDocument.Open(pdfStream))
            {
                foreach (var page in document.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
            }
            return Task.FromResult(sb.ToString());
        }
    }
}
