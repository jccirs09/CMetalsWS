using CMetalsWS.Data;
using CMetalsWS.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CMetalsWS.Tests
{
    public class PdfParsingServiceTests
    {
        private readonly PdfParsingService _service;

        public PdfParsingServiceTests()
        {
            _service = new PdfParsingService(new NullLogger<PdfParsingService>());
        }

        [Theory]
        [InlineData("15355234.pdf")]
        [InlineData("15362004.pdf")]
        [InlineData("15374219.pdf")]
        [InlineData("15384525.pdf")]
        [InlineData("15392152.pdf")]
        [InlineData("sales order sample 1.pdf")]
        [InlineData("sales order sample 2.pdf")]
        public async Task ParsePdf(string fileName)
        {
            var pdfBytes = await File.ReadAllBytesAsync($"Samples/{fileName}");
            var (header, items) = await _service.ParsePickingListAsync(pdfBytes);

            Assert.NotNull(header);
            Assert.NotEmpty(items);

            System.Console.WriteLine($"--- {fileName} ---");
            System.Console.WriteLine($"Sales Order: {header.SalesOrderNumber}");
            System.Console.WriteLine($"Sold To: {header.SoldTo}");
            System.Console.WriteLine($"Ship To: {header.ShipTo}");
            System.Console.WriteLine($"Ship Date: {header.ShipDate}");
            System.Console.WriteLine($"FOB: {header.FOB}");
            System.Console.WriteLine($"Shipping Via: {header.ShippingVia}");
            System.Console.WriteLine($"Total Weight: {header.TotalWeight}");
            System.Console.WriteLine($"Item Count: {items.Count}");
            System.Console.WriteLine($"--- END {fileName} ---");
        }

        [Fact]
        public async Task DebugPdfLayout()
        {
            var fileName = "15374219.pdf";
            var pdfBytes = await File.ReadAllBytesAsync($"Samples/{fileName}");
            using var doc = UglyToad.PdfPig.PdfDocument.Open(pdfBytes);
            var page = doc.GetPage(1);
            var lines = PdfParsingService.GetLines(page);

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                System.Console.WriteLine($"Line {i} (Y={line.Y}): {line.RawLineText}");
                foreach (var word in line.Words)
                {
                    System.Console.WriteLine($"  - Word: '{word.Raw}' (Norm: '{word.Norm}') [X={word.X}, Y={word.Y}] Box={word.Box}");
                }
            }
        }
    }
}
