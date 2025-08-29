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

            // Sales order / picking number (very permissive: digits with optional dash)
            pl.SalesOrderNumber = MatchFirst(lines, @"(?:PICKING\s+LIST\s+No\.?|No\.)\s*(?<num>[0-9A-Z\-]+)", "num") ?? string.Empty;

            // Dates
            pl.ShipDate = ParseDate(MatchFirst(lines, @"\bSHIP\s*DATE\b\s*(?<dt>\d{2}/\d{2}/\d{4})", "dt"));
            // Try explicit ORDER DATE first, then fall back to PRINT DATE/TIME, else now
            var orderDateText = MatchFirst(lines, @"\bORDER\s*DATE\b\s*(?<dt>\d{2}/\d{2}/\d{4})", "dt")
                                ?? MatchFirst(lines, @"\bPRINT\s*DATE/TIME\b\s*(?<dt>\d{2}/\d{2}/\d{4})", "dt");
            pl.OrderDate = ParseDate(orderDateText) ?? DateTime.UtcNow;

            // Sales rep
            pl.SalesRep = MatchFirst(lines, @"\bSALES\s*REP\b\s*(?<rep>[A-Z][A-Z \-'.]+)", "rep");

            // SOLD TO (CustomerName) — usually next non-empty line
            var soldToIdx = IndexOf(lines, l => l.Contains("SOLD TO", StringComparison.OrdinalIgnoreCase));
            if (soldToIdx >= 0)
            {
                var nm = NextNonEmpty(lines, soldToIdx + 1, takeUpTo: 1);
                if (!string.IsNullOrWhiteSpace(nm)) pl.CustomerName = nm;
            }

            // SHIP TO (multi-line address)
            var shipToIdx = IndexOf(lines, l => l.Contains("SHIP TO", StringComparison.OrdinalIgnoreCase));
            if (shipToIdx >= 0)
            {
                var addr = NextNonEmptyBlock(lines, shipToIdx + 1, maxLines: 4,
                    stopIf: s => IsHeaderOrFooter(s) || s.StartsWith("SHIP VIA", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(addr)) pl.ShipToAddress = addr;
            }

            // Ship Via
            var via = MatchFirst(lines, @"\bSHIP\s*VIA\b\s*(?<via>.+?)(?:\s{2,}|$)", "via");
            if (!string.IsNullOrWhiteSpace(via)) pl.ShippingMethod = via.Trim();

            // Items
            ParseItems(lines, pl.Items);

            // Fallback: if no CustomerName and we have a ShipTo first line, use it
            if (string.IsNullOrWhiteSpace(pl.CustomerName) && shipToIdx >= 0)
            {
                var firstShipLine = NextNonEmpty(lines, shipToIdx + 1, takeUpTo: 1);
                if (!string.IsNullOrWhiteSpace(firstShipLine)) pl.CustomerName = firstShipLine;
            }

            return pl;
        }

        private static void ParseItems(List<string> lines, ICollection<PickingListItem> items)
        {
            // Find header for the items table
            var headerIdx = IndexOf(lines, l => l.StartsWith("LINE", StringComparison.OrdinalIgnoreCase) &&
                                                l.Contains("DESCRIPTION", StringComparison.OrdinalIgnoreCase));
            if (headerIdx < 0) return;

            var i = headerIdx + 1;
            while (i < lines.Count)
            {
                var raw = lines[i];
                if (string.IsNullOrWhiteSpace(raw)) { i++; continue; }
                if (IsHeaderOrFooter(raw)) break;

                // Lines that are definitely not item starts
                if (raw.StartsWith("TAG", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("SOURCE:", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("LOC", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("- CTL -", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("CTL", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("- SHEET STOCK -", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("Other Reservations", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    continue;
                }

                // Try to match an item header line:
                //  <line> <qty> <unit>  ...  <itemId>   <width>"   <length>"   <weight>
                // Support PCS or LBS. Length and/or width may be absent for coils.
                var m = Regex.Match(raw,
                    @"^(?<line>\d+)\s+(?<qty>[\d,]+)\s+(?<unit>PCS|EA|LBS)\b.*?(?<item>[A-Z0-9\-_\/\.]+)\s*(?<sizes>.*)$",
                    RegexOptions.IgnoreCase);

                if (!m.Success)
                {
                    // If it doesn't look like an item start, move on
                    i++;
                    continue;
                }

                var lineNum = ToInt(m.Groups["line"].Value) ?? 0;
                var qty = ToDec(m.Groups["qty"].Value) ?? 0m;
                var unit = m.Groups["unit"].Value.ToUpperInvariant();
                var itemId = m.Groups["item"].Value.Trim();
                var sizesTail = m.Groups["sizes"].Value;

                decimal? width = null, length = null, weight = null;

                // Try to parse sizes from the tail: width" length" weight
                // Examples:
                //   60" 120" 3,624
                //   60" 41.75" 2,364.094
                //   60" 939.031        (coil: width then weight)
                //   48" 96"            (sheet without weight on this line; may appear elsewhere)
                var sizeWeight = Regex.Match(sizesTail,
                    @"(?:(?<w>\d{1,3}(?:\.\d{1,3})?)\"")?\s*(?:(?<l>\d{1,3}(?:\.\d{1,3})?)\"")?\s*(?<wt>\d{1,3}(?:,\d{3})*(?:\.\d+)?)?\s*$",
                    RegexOptions.IgnoreCase);

                if (sizeWeight.Success)
                {
                    var w = sizeWeight.Groups["w"]; var l = sizeWeight.Groups["l"]; var wt = sizeWeight.Groups["wt"];
                    if (w.Success) width = ToDec(w.Value);
                    if (l.Success) length = ToDec(l.Value);
                    if (wt.Success) weight = ToDec(wt.Value);
                }

                // Read description lines below the header row until the next item or a section break
                var desc = new StringBuilder();
                var j = i + 1;
                while (j < lines.Count)
                {
                    var s = lines[j];
                    if (string.IsNullOrWhiteSpace(s)) { j++; continue; }
                    if (IsHeaderOrFooter(s)) break;
                    if (Regex.IsMatch(s, @"^\d+\s+[\d,]+\s+(PCS|EA|LBS)\b", RegexOptions.IgnoreCase)) break; // next item
                    if (s.StartsWith("TAG", StringComparison.OrdinalIgnoreCase)) break;
                    if (s.StartsWith("SOURCE:", StringComparison.OrdinalIgnoreCase)) break;
                    if (s.StartsWith("- CTL -", StringComparison.OrdinalIgnoreCase) || s.StartsWith("CTL", StringComparison.OrdinalIgnoreCase)) break;
                    if (s.StartsWith("- SHEET STOCK -", StringComparison.OrdinalIgnoreCase)) break;

                    desc.Append(' ').Append(s.Trim());
                    j++;
                }

                var item = new PickingListItem
                {
                    LineNumber = lineNum,
                    Quantity = qty,
                    Unit = unit,
                    ItemId = string.IsNullOrWhiteSpace(itemId) ? InferItemIdFromDesc(desc.ToString()) : itemId,
                    ItemDescription = Clean(desc.ToString()),
                    Width = width,
                    Length = length,
                    Weight = weight
                };

                items.Add(item);

                i = j;
            }
        }

        private static string ExtractText(Stream pdfStream)
        {
            // Make sure the stream is at the beginning
            if (pdfStream.CanSeek) pdfStream.Position = 0;

            // Avoid using statements to prevent IDisposable API mismatch issues
            var ocr = new IronTesseract();
            var input = new OcrInput();

            // Works across many IronOCR versions
            input.LoadPdf(pdfStream);

            // If your version supports it, these can help quality:
            // input.Configuration.DetectWhiteTextOnDarkBackgrounds = true;
            // input.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.Auto;
            // input.Tesseract.Version = TesseractVersion.Tesseract5;

            var result = ocr.Read(input);
            return result?.Text ?? string.Empty;
        }

        // ---------- Helpers ----------

        private static bool IsHeaderOrFooter(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (s.StartsWith("LINE", StringComparison.OrdinalIgnoreCase) && s.Contains("DESCRIPTION", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("PICKING LIST No.", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("PRINT DATE/TIME", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("PULLED BY TOTAL WT", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("TERMS", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Contains("MAX SKID WEIGHT", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Contains("MAX COIL WEIGHT", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("----------------", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("********************************", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("================================", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static List<string> SplitLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            return text
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();
        }

        private static string? MatchFirst(IEnumerable<string> lines, string pattern, string groupName)
        {
            foreach (var l in lines)
            {
                var m = Regex.Match(l, pattern, RegexOptions.IgnoreCase);
                if (m.Success) return m.Groups[groupName].Value.Trim();
            }
            return null;
        }

        private static int IndexOf(List<string> lines, Func<string, bool> pred)
        {
            for (int i = 0; i < lines.Count; i++)
                if (pred(lines[i])) return i;
            return -1;
        }

        private static string? SafeGet(List<string> lines, int index) =>
            (index >= 0 && index < lines.Count) ? lines[index] : null;

        private static string NextNonEmpty(List<string> lines, int start, int takeUpTo = 1)
        {
            var taken = new List<string>();
            int i = start;
            while (i < lines.Count && taken.Count < takeUpTo)
            {
                var s = lines[i].Trim();
                if (!string.IsNullOrWhiteSpace(s) && !IsHeaderOrFooter(s)) taken.Add(s);
                i++;
            }
            return string.Join(" ", taken);
        }

        private static string NextNonEmptyBlock(List<string> lines, int start, int maxLines, Func<string, bool>? stopIf = null)
        {
            var taken = new List<string>();
            int i = start;
            while (i < lines.Count && taken.Count < maxLines)
            {
                var s = lines[i].Trim();
                if (stopIf != null && stopIf(s)) break;
                if (string.IsNullOrWhiteSpace(s)) { i++; continue; }
                if (IsHeaderOrFooter(s)) break;
                taken.Add(s);
                i++;
            }
            return string.Join(", ", taken);
        }

        private static DateTime? ParseDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParseExact(s.Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            if (DateTime.TryParse(s, out dt)) return dt;
            return null;
        }

        private static int? ToInt(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (int.TryParse(s.Replace(",", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) return v;
            return null;
        }

        private static decimal? ToDec(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var cleaned = s.Replace(",", "").Replace("\"", "");
            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.CurrentCulture, out v)) return v;
            return null;
        }

        private static string Clean(string s) => Regex.Replace(s ?? string.Empty, @"\s+", " ").Trim();

        private static string InferItemIdFromDesc(string desc)
        {
            if (string.IsNullOrWhiteSpace(desc)) return string.Empty;
            var first = desc.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            // If the first token is clearly not an ID, keep empty
            return Regex.IsMatch(first, @"^[A-Z0-9\-_\/\.]+$") ? first : string.Empty;
        }
    }
}
