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

            pl.SalesOrderNumber = MatchFirst(lines, @"No\.\s*(?<num>[0-9A-Za-z\-]+)", "num") ?? string.Empty;

            var shipDt = MatchFirst(lines, @"SHIP\s+DATE\s+(?<dt>\d{2}/\d{2}/\d{4})", "dt");
            pl.ShipDate = ParseDate(shipDt);

            var orderDt = MatchFirst(lines, @"ORDER\s+DATE\s+(?<dt>\d{2}/\d{2}/\d{4})", "dt");
            pl.OrderDate = ParseDate(orderDt) ?? DateTime.UtcNow;

            var soldToBlock = ExtractBlock(lines, "SOLD TO", "SHIP TO");
            pl.CustomerName = soldToBlock.FirstOrDefault()?.Trim();

            var shipToBlock = ExtractBlock(lines, "SHIP TO", "PICKING GROUP");
            pl.ShipToAddress = string.Join(", ", shipToBlock).Trim();

            ParseItems(lines, pl.Items);

            return pl;
        }

        private static void ParseItems(List<string> lines, ICollection<PickingListItem> items)
        {
            var header = IndexOf(lines, l => l.Contains("LINE") && l.Contains("QUANTITY") && l.Contains("DESCRIPTION"));
            if (header < 0) return;

            var i = header + 1;
            while (i < lines.Count)
            {
                var raw = lines[i];
                if (string.IsNullOrWhiteSpace(raw) || IsHeaderOrFooter(raw) || raw.Length == 0 || !char.IsDigit(raw[0]))
                {
                    i++;
                    continue;
                }

                var m = Regex.Match(raw, @"^(?<line>\d+)\s+(?<qty>[\d,]+\s+LBS)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    var lineNum = ToInt(m.Groups["line"].Value) ?? 0;
                    var qtyParts = m.Groups["qty"].Value.Split(' ');
                    var qty = ToDec(qtyParts[0]);
                    var unit = qtyParts[1];

                    var endValues = Regex.Match(raw, @"(?<width>[\d\.,\s]+)"")?\s+(?<weight>[\d,]+)$");
                    var width = ToDec(endValues.Groups["width"].Value);
                    var weight = ToDec(endValues.Groups["weight"].Value);

                    var descriptionBlock = new StringBuilder();
                    var descLines = 0;
                    for (var j = i + 1; j < lines.Count; j++)
                    {
                        var nextLine = lines[j];
                        if (string.IsNullOrWhiteSpace(nextLine) || IsHeaderOrFooter(nextLine) || (nextLine.Length > 0 && char.IsDigit(nextLine[0]) && Regex.IsMatch(nextLine, @"^\d+\s")))
                        {
                            break;
                        }
                        descriptionBlock.AppendLine(nextLine.Trim());
                        descLines++;
                    }

                    var fullDescription = Clean(descriptionBlock.ToString());
                    var itemId = fullDescription.Split(new[] { '
', '
' }).FirstOrDefault()?.Trim() ?? string.Empty;

                    var pli = new PickingListItem
                    {
                        LineNumber = lineNum,
                        Quantity = qty ?? 0,
                        Unit = unit,
                        ItemId = itemId,
                        ItemDescription = fullDescription,
                        Width = width,
                        Weight = weight
                    };
                    items.Add(pli);

                    i += descLines + 1;
                    continue;
                }

                i++;
            }
        }

        private static string ExtractText(Stream pdfStream)
        {
            var ocr = new IronTesseract();
            using var input = new OcrInput(pdfStream);
            var result = ocr.Read(input);
            return result.Text;
        }

        private static List<string> ExtractBlock(List<string> lines, string startMarker, string endMarker)
        {
            var startIdx = IndexOf(lines, l => l.Contains(startMarker));
            var endIdx = IndexOf(lines, l => l.Contains(endMarker));
            if (startIdx < 0 || endIdx < 0 || endIdx <= startIdx) return new List<string>();

            return lines.Skip(startIdx + 1).Take(endIdx - startIdx - 1).Select(l => l.Trim()).ToList();
        }

        private static bool IsHeaderOrFooter(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (s.Contains("PULLED BY") || s.Contains("TOTAL WT")) return true;
            return false;
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
        private static DateTime? ParseDate(string? s) => DateTime.TryParseExact(s?.Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : null;
        private static int? ToInt(string s) => int.TryParse(s.Replace(",", ""), out var v) ? v : null;
        private static decimal? ToDec(string? s) => decimal.TryParse(s?.Replace(",", "").Replace(""", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
        private static string Clean(string s) => Regex.Replace(s ?? string.Empty, @"\s+", " ").Trim();
    }
}