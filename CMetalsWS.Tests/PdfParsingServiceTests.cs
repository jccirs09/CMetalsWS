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
            System.Console.WriteLine($"Total Weight: {header.TotalWeight}");
            System.Console.WriteLine($"Item Count: {items.Count}");
            System.Console.WriteLine($"--- END {fileName} ---");
        }

    }
}
