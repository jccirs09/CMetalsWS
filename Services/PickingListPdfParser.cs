using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CMetalsWS.Data;
using IronOcr;

namespace CMetalsWS.Services
{
    public class PickingListPdfParser : IPickingListPdfParser
    {
        public PickingList Parse(Stream pdfStream, int branchId, int? customerId = null, int? truckId = null)
        {
            var text = ExtractText(pdfStream);
            var lines = SplitLines(text);

            var pl = new PickingList
            {
                BranchId = branchId,
                CustomerId = customerId,
                TruckId = truckId
            };

            // Header fields are more reliable with specific regex
            pl.SalesOrderNumber = MatchFirst(lines, @"No\.\s*(?<num>[\d-A-Z]+)", "num") ?? string.Empty;
            pl.ShipDate = ParseDate(MatchFirst(lines, @"SHIP DATE\s+(?<dt>\d{2}/\d{2}/\d{4})", "dt"));
            pl.OrderDate = ParseDate(MatchFirst(lines, @"ORDER DATE\s+(?<dt>\d{2}/\d{2}/\d{4})", "dt")) ?? DateTime.UtcNow;

            // Customer and ShipTo are in blocks. Find the line containing the label, then take the next non-empty line.
            var soldToLine = lines.FirstOrDefault(l => l.Contains("SOLD TO"));
            if (soldToLine != null)
            {
                var soldToIdx = lines.IndexOf(soldToLine);
                pl.CustomerName = SafeGet(lines, soldToIdx + 1)?.Trim();
            }

            var shipToLine = lines.FirstOrDefault(l => l.Contains("SHIP TO"));
            if (shipToLine != null)
            {
                var shipToIdx = lines.IndexOf(shipToLine);
                var addressLines = lines.Skip(shipToIdx + 1).Take(3).Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l));
                pl.ShipToAddress = string.Join(" ", addressLines);
            }

            ParseItems(lines, pl.Items);

            return pl;
        }

        private static void ParseItems(List<string> lines, ICollection<PickingListItem> items)
        {
            var headerIdx = IndexOf(lines, l => l.Contains("LINE") && l.Contains("QUANTITY") && l.Contains("DESCRIPTION"));
            if (headerIdx < 0) return;

            var currentLine = headerIdx + 1;
            while (currentLine < lines.Count)
            {
                var line = lines[currentLine];
                if (IsFooter(line)) break;

                // Find the start of a new item (must start with a line number)
                var itemMatch = Regex.Match(line, @"^\s*(?<lineNum>\d+)\s+(?<qty>[\d,]+)\s+(?<unit>LBS|PCS|EA)", RegexOptions.IgnoreCase);
                if (!itemMatch.Success)
                {
                    currentLine++;
                    continue;
                }

                // We found a new item, now gather all its data
                var pli = new PickingListItem
                {
                    LineNumber = ToInt(itemMatch.Groups["lineNum"].Value) ?? 0,
                    Quantity = ToDec(itemMatch.Groups["qty"].Value) ?? 0,
                    Unit = itemMatch.Groups["unit"].Value
                };

                // The rest of the first line is part of the description
                var descriptionContent = new StringBuilder();
                descriptionContent.AppendLine(line.Substring(itemMatch.Length).Trim());

                // Find the end of the line to get width and weight
                var endOfLineMatch = Regex.Match(line, @"(?<width>[\d\.]+)"")?\s+(?<weight>[\d,]+)\s*$", RegexOptions.IgnoreCase);
                if (endOfLineMatch.Success)
                {
                    pli.Width = ToDec(endOfLineMatch.Groups["width"].Value);
                    pli.Weight = ToDec(endOfLineMatch.Groups["weight"].Value);
                }

                // Consume subsequent lines that are part of the description
                var descLineIdx = currentLine + 1;
                while (descLineIdx < lines.Count)
                {
                    var descLine = lines[descLineIdx];
                    if (IsFooter(descLine) || Regex.IsMatch(descLine, @"^\s*\d+\s+")) break;
                    descriptionContent.AppendLine(descLine.Trim());
                    descLineIdx++;
                }

                var fullDescription = Clean(descriptionContent.ToString());
                pli.ItemDescription = fullDescription;

                // Extract ItemId from the description (usually the first "word")
                var firstWord = fullDescription.Split(new[] {' ', '
', '
'}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                pli.ItemId = firstWord ?? string.Empty;

                items.Add(pli);
                currentLine = descLineIdx;
            }
        }

        private static string ExtractText(Stream pdfStream)
        {
            var ocr = new IronTesseract();
            using var input = new OcrInput(pdfStream);
            var result = ocr.Read(input);
            return result.Text;
        }

        private static bool IsFooter(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return s.Contains("PULLED BY") || s.Contains("TOTAL WT");
        }

        private static List<string> SplitLines(string text) => text.Replace("
", "
").Split('
').Select(l => l.Trim()).ToList();
        private static int IndexOf(List<string> lines, Func<string, bool> pred) { for (int i = 0; i < lines.Count; i++) if (pred(lines[i])) return i; return -1; }
        private static string? MatchFirst(IEnumerable<string> lines, string pattern, string groupName)
        {
            foreach (var l in lines)
            {
                var m = Regex.Match(l, pattern, RegexOptions.IgnoreCase);
                if (m.Success) return m.Groups[groupName].Value.Trim();
            }
            return null;
        }
        private static string? SafeGet(List<string> lines, int index) => (index >= 0 && index < lines.Count) ? lines[index] : null;
        private static DateTime? ParseDate(string? s) => DateTime.TryParseExact(s?.Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : null;
        private static int? ToInt(string? s) => int.TryParse(s?.Replace(",", ""), out var v) ? v : null;
        private static decimal? ToDec(string? s) => decimal.TryParse(s?.Replace(",", "").Replace(""", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
        private static string Clean(string s) => Regex.Replace(s ?? string.Empty, @"\s+", " ").Trim();
    }
}