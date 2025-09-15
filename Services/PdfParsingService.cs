using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;

namespace CMetalsWS.Services
{
    public class PdfParsingService : IPdfParsingService
    {
        private readonly ILogger<PdfParsingService> _logger;

        public PdfParsingService(ILogger<PdfParsingService> logger)
        {
            _logger = logger;
        }

        public Task<(PickingList, List<PickingListItem>)> ParsePickingListAsync(byte[] pdfBytes)
        {
            using (var document = PdfDocument.Open(pdfBytes))
            {
                var header = ParseHeader(document);
                var lineItems = ParseLineItems(document);
                return Task.FromResult((header, lineItems));
            }
        }

        private PickingList ParseHeader(PdfDocument document)
        {
            var page = document.GetPage(1);
            var words = page.GetWords().ToList();

            var pickingList = new PickingList
            {
                SalesOrderNumber = GetValueInArea(words, new PdfRectangle(100, 750, 200, 770)) ?? string.Empty,
                OrderDate = GetDateValueInArea(words, new PdfRectangle(100, 730, 200, 750)),
                ShipDate = GetDateValueInArea(words, new PdfRectangle(100, 710, 200, 730)),
                SoldTo = GetValueInArea(words, new PdfRectangle(50, 600, 300, 700)) ?? string.Empty,
                ShipTo = GetValueInArea(words, new PdfRectangle(350, 600, 600, 700)) ?? string.Empty,
                SalesRep = GetValueInArea(words, new PdfRectangle(100, 580, 200, 600)) ?? string.Empty,
                ShippingVia = GetValueInArea(words, new PdfRectangle(100, 560, 200, 580)) ?? string.Empty,
                FOB = GetValueInArea(words, new PdfRectangle(100, 540, 200, 560)) ?? string.Empty,
                Buyer = GetValueInArea(words, new PdfRectangle(100, 520, 200, 540)),
                PrintDateTime = GetDateTimeValueInArea(words, new PdfRectangle(400, 750, 550, 770)),
                TotalWeight = GetDecimalValueInArea(words, new PdfRectangle(400, 520, 550, 540)) ?? 0m
            };

            return pickingList;
        }

        private List<PickingListItem> ParseLineItems(PdfDocument document)
        {
            var lineItems = new List<PickingListItem>();
            foreach (var page in document.GetPages())
            {
                var words = page.GetWords().ToList();
                // Define column boundaries based on header positions
                var headers = new Dictionary<string, PdfRectangle?>();
                headers["Line"] = FindHeader(words, "Line");
                headers["Qty"] = FindHeader(words, "Qty");
                headers["Item"] = FindHeader(words, "Item");
                headers["Description"] = FindHeader(words, "Description");
                headers["Width"] = FindHeader(words, "Width");
                headers["Length"] = FindHeader(words, "Length");
                headers["Weight"] = FindHeader(words, "Weight");
                headers["Unit"] = FindHeader(words, "Unit");

                if (headers["Line"] == null) continue;

                var tableTop = headers["Line"]!.Value.Top;
                var lines = page.GetWords()
                    .Where(w => w.BoundingBox.Bottom < tableTop)
                    .GroupBy(w => Math.Round(w.BoundingBox.Centroid.Y, 0))
                    .OrderByDescending(g => g.Key)
                    .Select(g => g.OrderBy(w => w.BoundingBox.Left).ToList())
                    .ToList();

                foreach (var line in lines)
                {
                    if (line.Count < 2) continue; // Skip empty lines

                    var item = new PickingListItem
                    {
                        LineNumber = int.TryParse(GetValueForColumn(line, headers, "Line"), out var ln) ? ln : 0,
                        Quantity = decimal.TryParse(GetValueForColumn(line, headers, "Qty"), out var qty) ? qty : 0,
                        ItemId = GetValueForColumn(line, headers, "Item") ?? string.Empty,
                        ItemDescription = GetValueForColumn(line, headers, "Description") ?? string.Empty,
                        Width = decimal.TryParse(GetValueForColumn(line, headers, "Width"), out var w) ? w : 0,
                        Length = decimal.TryParse(GetValueForColumn(line, headers, "Length"), out var l) ? l : 0,
                        Weight = decimal.TryParse(GetValueForColumn(line, headers, "Weight"), out var wt) ? wt : 0,
                        Unit = GetValueForColumn(line, headers, "Unit") ?? "EA"
                    };
                    if (item.LineNumber > 0)
                    {
                        lineItems.Add(item);
                    }
                }
            }
            return lineItems;
        }

        private PdfRectangle? FindHeader(List<Word> words, string headerText)
        {
            return words.FirstOrDefault(w => w.Text.Equals(headerText, StringComparison.OrdinalIgnoreCase))?.BoundingBox;
        }

        private string? GetValueForColumn(List<Word> line, Dictionary<string, PdfRectangle?> headers, string columnName)
        {
            if (!headers.TryGetValue(columnName, out var headerBox) || headerBox == null)
            {
                return null;
            }

            var columnBox = headerBox.Value;
            var wordsInColumn = line.Where(w =>
            {
                var wordBox = w.BoundingBox;
                // Check if the horizontal center of the word falls within the horizontal bounds of the header
                var wordCenter = wordBox.Left + wordBox.Width / 2;
                return wordCenter >= columnBox.Left && wordCenter <= columnBox.Right;
            });

            return string.Join(" ", wordsInColumn.Select(w => w.Text));
        }


        private string? GetValueInArea(List<Word> words, PdfRectangle area)
        {
            var wordsInArea = words.Where(w => area.IntersectsWith(w.BoundingBox));
            return string.Join(" ", wordsInArea.Select(w => w.Text));
        }

        private DateTime? GetDateValueInArea(List<Word> words, PdfRectangle area)
        {
            var value = GetValueInArea(words, area);
            if (DateTime.TryParse(value, out var date))
            {
                return date;
            }
            return null;
        }

        private DateTime? GetDateTimeValueInArea(List<Word> words, PdfRectangle area)
        {
            var value = GetValueInArea(words, area);
            if (DateTime.TryParse(value, out var date))
            {
                return date;
            }
            return null;
        }

        private decimal? GetDecimalValueInArea(List<Word> words, PdfRectangle area)
        {
            var value = GetValueInArea(words, area);
            if (decimal.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }
    }
}
