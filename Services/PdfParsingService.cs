using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace CMetalsWS.Services
{
    public class PdfParsingService : IPdfParsingService
    {
        private readonly ILogger<PdfParsingService> _logger;

        public PdfParsingService(ILogger<PdfParsingService> logger)
        {
            _logger = logger;
        }

        public async Task<PickingList> ParseAsync(Stream pdfStream, string sourceFileName)
        {
            var text = new StringBuilder();
            int pageCount = 0;
            using (var pdf = PdfDocument.Open(pdfStream))
            {
                pageCount = pdf.NumberOfPages;
                foreach (var page in pdf.GetPages())
                {
                    text.AppendLine(page.Text);
                }
            }

            var fullText = text.ToString();

            var salesOrderNumber = ExtractValue(fullText, @"PICKING\s*LIST\s*No\.?\s*(\d+)") ?? ExtractValue(fullText, @"\b\d{7,}\b");
            if(salesOrderNumber == null)
            {
                throw new InvalidOperationException("Could not parse Sales Order Number.");
            }

            var pickingList = new PickingList
            {
                SalesOrderNumber = salesOrderNumber,
                PageCount = pageCount,
                Status = PickingListStatus.Pending,
                Items = new List<PickingListItem>()
            };

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fullText));
                pickingList.RawTextHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }

            // Header parsing
            pickingList.PrintDateTime = ParseDate(ExtractValue(fullText, @"PRINT\s*DATE/TIME:\s*(\d{1,2}/\d{1,2}/\d{4}\s+\d{1,2}:\d{2}(?::\d{2})?\s*(?:AM|PM)?)"));
            pickingList.ShipDate = ParseDate(ExtractValue(fullText, @"SHIP\s*DATE[:\s]\s*(\d{1,2}/\d{1,2}/\d{4})"));
            pickingList.OrderDate = ParseDate(ExtractValue(fullText, @"ORDER\s*DATE[:\s]?\s*(\d{1,2}/\d{1,2}/\d{4})"));
            pickingList.Buyer = ExtractValue(fullText, @"BUYER:\s*(.*)");
            pickingList.SalesRep = ExtractValue(fullText, @"SALES\s*REP:\s*(.*)");
            pickingList.ShippingVia = ExtractValue(fullText, @"SHIP\s*VIA:\s*(.*)");
            pickingList.FOB = ExtractValue(fullText, @"FOB\s*POINT:\s*(.*)");
            pickingList.SoldTo = ExtractBlock(fullText, "SOLD TO:", "SHIP TO:");
            pickingList.ShipTo = ExtractBlock(fullText, "SHIP TO:", "Line");
            pickingList.ParseNotes = ExtractNotes(fullText);

            // Line item parsing
            var lines = fullText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lineItemRegex = new Regex(@"^\s*(\d{1,3})\s+.*$");

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (lineItemRegex.IsMatch(line))
                {
                    var item = ParseLineItem(line);
                    if (item != null)
                    {
                        var notes = new List<string>();
                        for (int j = i + 1; j < lines.Length; j++)
                        {
                            var nextLine = lines[j].Trim();
                            if (lineItemRegex.IsMatch(nextLine) ||
                                nextLine.StartsWith("SOURCE:", StringComparison.OrdinalIgnoreCase) ||
                                nextLine.StartsWith("TAG #", StringComparison.OrdinalIgnoreCase) ||
                                nextLine.StartsWith("Other Reservations:", StringComparison.OrdinalIgnoreCase) ||
                                nextLine.StartsWith("PULLED BY", StringComparison.OrdinalIgnoreCase))
                            {
                                if (nextLine.Contains("TAG #") && nextLine.Contains("HEAT #") && nextLine.Contains("MILL REF #"))
                                {
                                    item.HasTagLots = true;
                                }
                                break;
                            }
                            notes.Add(nextLine);
                        }
                        item.SalesNote = string.Join("\n", notes);
                        pickingList.Items.Add(item);
                    }
                }
            }

            pickingList.HasParseIssues = pickingList.Items.Any(i => i.NeedsAttention);

            return await Task.FromResult(pickingList);
        }

        private PickingListItem? ParseLineItem(string line)
        {
            try
            {
                var lineItemRegex = new Regex(
                    @"^\s*(?<LineNumber>\d{1,3})\s+" +
                    @"(?<Quantity>[\d,]+\.?\d*)\s+" +
                    @"(?<Unit>[A-Za-z]+)\s+" +
                    @"(?:QTY\s+STAGED\s+_{5,}\s+)?" +
                    @"(?<ItemId>[^\s]+)\s+" +
                    @"(?<ItemDescription>.*?)\s+" +
                    @"(?<Width>[\d\.,\""]+)\s+" +
                    @"(?<Length>[\d\.,\""]*)\s+" +
                    @"(?<Weight>[\d\.,\""]+)\s*$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

                var match = lineItemRegex.Match(line);
                if (!match.Success) return null;

                var item = new PickingListItem
                {
                    LineNumber = int.Parse(match.Groups["LineNumber"].Value),
                    Quantity = ParseDecimal(match.Groups["Quantity"].Value) ?? 0,
                    Unit = match.Groups["Unit"].Value.ToUpperInvariant(),
                    ItemId = match.Groups["ItemId"].Value,
                    ItemDescription = match.Groups["ItemDescription"].Value.Trim(),
                    Width = ParseDecimal(match.Groups["Width"].Value),
                    Length = ParseDecimal(match.Groups["Length"].Value),
                    Weight = ParseDecimal(match.Groups["Weight"].Value)
                };

                if (item.Quantity == 0 || item.Width == 0 || item.Weight == 0)
                {
                    item.NeedsAttention = true;
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse line item: {Line}", line);
                return null;
            }
        }

        private string? ExtractValue(string text, string pattern)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private string ExtractBlock(string text, string startToken, string endToken)
        {
            var startMatch = Regex.Match(text, Regex.Escape(startToken), RegexOptions.IgnoreCase);
            if (!startMatch.Success) return "";
            var startIndex = startMatch.Index + startMatch.Length;

            var endMatch = Regex.Match(text, Regex.Escape(endToken), RegexOptions.IgnoreCase);
            var endIndex = endMatch.Success ? endMatch.Index : text.Length;

            return text.Substring(startIndex, endIndex - startIndex).Trim();
        }

        private string ExtractNotes(string text)
        {
            var notes = new List<string>();
            var lines = text.Split('\n');
            foreach(var line in lines)
            {
                if (line.Trim().StartsWith("MAX SKID WEIGHT", StringComparison.OrdinalIgnoreCase) || line.Trim().StartsWith("RECEIVING HOURS", StringComparison.OrdinalIgnoreCase))
                {
                    notes.Add(line.Trim());
                }
            }
            return string.Join("\n", notes);
        }

        private DateTime? ParseDate(string? dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return null;
            if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return null;
        }

        private decimal? ParseDecimal(string? decStr)
        {
            if (string.IsNullOrWhiteSpace(decStr)) return null;
            var cleaned = decStr.Replace("\"", "").Replace(",", "").Trim();
            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return Math.Round(result, 3);
            }
            return null;
        }
    }
}
