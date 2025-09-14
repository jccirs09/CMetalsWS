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

        private static readonly string[] NL = new[] { "\r\n", "\n" };

        private static readonly Regex RowRx = new(
            @"^\s*(?<LineNumber>\d{1,3})\s+" +
            @"(?<Quantity>[\d,]+(?:\.\d+)?)\s+" +
            @"(?<Unit>[A-Za-z]+)\s+" +
            @"(?:QTY\s+STAGED\s+[_\s]+)?" +
            @"(?<ItemId>\S+)\s+" +
            @"(?<ItemDescription>.+?)\s+" +
            @"(?<Width>\d+(?:\.\d+)?)[\""]?\s+" +
            @"(?<Length>\d*(?:\.\d+)?)[\""]?\s+" +
            @"(?<Weight>[\d,]+(?:\.\d+)?)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public async Task<PickingList> ParseAsync(Stream pdfStream, string sourceFileName)
        {
            var sb = new StringBuilder();
            int pageCount;
            using (var pdf = PdfDocument.Open(pdfStream))
            {
                pageCount = pdf.NumberOfPages;
                foreach (var p in pdf.GetPages())
                    sb.AppendLine(p.Text);
            }

            var fullText = sb.ToString();
            var headerSeg = GetHeaderSegment(fullText);

            var so = ExtractFirst(fullText, @"PICKING\s*LIST\s*No\.?\s*(\d+)")
                     ?? ExtractFirst(fullText, @"\b\d{7,}\b")
                     ?? throw new InvalidOperationException("Could not parse Sales Order Number.");

            var pickingList = new PickingList
            {
                SalesOrderNumber = so,
                SourceFileName = sourceFileName,
                PageCount = pageCount,
                Status = PickingListStatus.Pending,
                Items = new List<PickingListItem>()
            };

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(fullText));
                pickingList.RawTextHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            // Header
            pickingList.PrintDateTime = ParseDateFlexible(
                ExtractFirst(headerSeg, @"(?im)PRINT\s*DATE/TIME:\s*(.+)$")
                ?? ExtractFirst(headerSeg, @"(?im)PRINT\s*DATE\/TIME\s*:\s*(.+)$"));

            var shipDateTxt = ExtractGridCell(headerSeg, @"(?i)PICKING\s+GROUP\s+BUYER\s+SHIP\s+DATE", "SHIP DATE")
                           ?? ExtractLabelInlineOrNextLine(headerSeg, "SHIP DATE");
            pickingList.ShipDate = ParseDateFlexible(shipDateTxt);

            pickingList.OrderDate = ParseDateFlexible(
                ExtractFirst(headerSeg, @"(?im)ORDER\s*DATE[:\s]?\s*(\d{1,2}/\d{1,2}/\d{4})")
                ?? ExtractLabelInlineOrNextLine(headerSeg, "ORDER DATE"));

            pickingList.SalesRep = ExtractGridCell(headerSeg, @"(?i)JOB\s+NAME\s+SALES\s+REP\s+SHIP\s+VIA", "SALES REP")
                                   ?? ExtractLabelInlineOrNextLine(headerSeg, "SALES REP");

            pickingList.ShippingVia = ExtractGridCell(headerSeg, @"(?i)JOB\s+NAME\s+SALES\s+REP\s+SHIP\s+VIA", "SHIP VIA")
                                   ?? ExtractLabelInlineOrNextLine(headerSeg, "SHIP VIA");

            pickingList.FOB = ExtractLabelInlineOrNextLine(headerSeg, "FOB POINT");
            pickingList.Buyer = ExtractLabelInlineOrNextLine(headerSeg, "BUYER");

            (pickingList.SoldTo, pickingList.ShipTo) = ExtractSoldToShipTo(headerSeg);

            pickingList.ParseNotes = string.Join("\n",
                headerSeg.Split(NL, StringSplitOptions.None)
                         .Select(s => s.Trim())
                         .Where(s => s.StartsWith("MAX SKID WEIGHT", StringComparison.OrdinalIgnoreCase)
                                  || s.StartsWith("RECEIVING HOURS", StringComparison.OrdinalIgnoreCase)));

            // Items
            var body = NormalizeBody(BodySegment(fullText));
            var lines = body.Split(NL, StringSplitOptions.RemoveEmptyEntries);

            var items = new List<PickingListItem>();

            // Pass 1 — regex
            for (int i = 0; i < lines.Length; i++)
            {
                var item = ParseLineItem(lines[i]);
                if (item == null) continue;

                var notes = new List<string>();
                for (int j = i + 1; j < lines.Length; j++)
                {
                    var next = lines[j].Trim();
                    if (RowRx.IsMatch(next)
                        || Regex.IsMatch(next, @"(?i)^(SOURCE:|TAG #|Other Reservations:|PULLED BY|TOTAL WT)"))
                    {
                        if (next.Contains("TAG #") && next.Contains("HEAT #") && next.Contains("MILL REF #"))
                            item.HasTagLots = true;
                        break;
                    }
                    notes.Add(next);
                }
                item.SalesNote = string.Join("\n", notes);
                items.Add(item);
            }

            // (Optional) Pass 2 — column-slice fallback could be added here if needed

            pickingList.Items = items;
            pickingList.HasParseIssues = items.Any(i => i.Quantity <= 0 || (i.Width ?? 0) <= 0 || (i.Weight ?? 0) <= 0);

            return await Task.FromResult(pickingList);
        }

        private static string GetHeaderSegment(string full)
        {
            var stop = Regex.Match(full, @"(?im)^\s*LINE\s+QUANTITY", RegexOptions.Multiline);
            return stop.Success ? full[..stop.Index] : full;
        }

        private static string BodySegment(string full)
        {
            var m = Regex.Match(full, @"(?ims)^\s*LINE\s+QUANTITY.+?(?<body>.+)$");
            return m.Success ? m.Groups["body"].Value : full;
        }

        private static string NormalizeBody(string body)
        {
            body = Regex.Replace(body, @"(?im)(?=^\s*\d{1,3}\s+[\d,])", "\n", RegexOptions.Multiline);
            body = Regex.Replace(body, @"(?im)(^|\s)(SOURCE:\s*STOCK)", "\n$2");
            body = Regex.Replace(body, @"(?im)^\s*(Other Reservations:|TAG #|PULLED BY|TOTAL WT)", "\n$0");
            return body;
        }

        private static string? ExtractFirst(string text, string pattern)
        {
            var m = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }

        private static string? ExtractLabelInlineOrNextLine(string text, string label)
        {
            var rxInline = new Regex($@"(?im)^\s*{Regex.Escape(label)}\s*:?\s*(?<v>.+?)\s*$");
            var m = rxInline.Match(text);
            if (m.Success) return m.Groups["v"].Value.Trim();
            return null;
        }

        private static string? ExtractGridCell(string text, string headerRowPattern, string label)
        {
            var lines = text.Split(NL, StringSplitOptions.None);
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var hdr = lines[i];
                if (!Regex.IsMatch(hdr, headerRowPattern, RegexOptions.IgnoreCase)) continue;

                var next = lines[i + 1];
                var tokens = Regex.Split(hdr.Trim(), @"\s{2,}").ToList();
                var starts = new List<(string token, int start)>();
                foreach (var t in tokens)
                {
                    var idx = hdr.IndexOf(t, StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0) starts.Add((t, idx));
                }
                starts = starts.OrderBy(s => s.start).ToList();

                var target = starts.FirstOrDefault(s => Regex.IsMatch(s.token, $"(?i)^{Regex.Escape(label)}$"));
                if (string.IsNullOrEmpty(target.token)) return null;

                var start = target.start;
                var end = starts.Where(s => s.start > start).Select(s => s.start).DefaultIfEmpty(hdr.Length).First();

                string Slice(string s, int a, int b) =>
                    a >= s.Length ? "" : s.Substring(a, Math.Min(s.Length, b) - a);

                return Slice(next, start, end).Trim();
            }
            return null;
        }

        private static (string soldTo, string shipTo) ExtractSoldToShipTo(string headerSeg)
        {
            var lines = headerSeg.Split(NL, StringSplitOptions.None);
            int hdrIdx = Array.FindIndex(lines, l =>
                Regex.IsMatch(l, @"(?i)\bSOLD\s+TO\b") &&
                Regex.IsMatch(l, @"(?i)\bSHIP\s+TO\b"));

            if (hdrIdx < 0) return ("", "");

            var hdr = lines[hdrIdx];
            var shipToIdx = hdr.IndexOf("SHIP TO", StringComparison.OrdinalIgnoreCase);
            if (shipToIdx < 0) shipToIdx = Math.Max(hdr.Length / 2, 20);

            var sold = new List<string>();
            var ship = new List<string>();

            for (int i = hdrIdx + 1; i < Math.Min(lines.Length, hdrIdx + 6); i++)
            {
                var ln = lines[i];
                if (Regex.IsMatch(ln, @"(?i)^\s*(FOB\s+POINT|SHIP\s+VIA|TERMS|LINE\s+QUANTITY)")) break;

                string left = ln.Length > shipToIdx ? ln[..shipToIdx] : ln;
                string right = ln.Length > shipToIdx ? ln[shipToIdx..] : "";

                left = left.Trim();
                right = right.Trim();

                if (!string.IsNullOrWhiteSpace(left)) sold.Add(left);
                if (!string.IsNullOrWhiteSpace(right)) ship.Add(right);
            }

            string Join(List<string> xs) => string.Join("\n", xs.Where(s => !string.IsNullOrWhiteSpace(s)).Take(4));
            return (Join(sold), Join(ship));
        }

        private static DateTime? ParseDateFlexible(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var formats = new[]
            {
                "M/d/yyyy", "MM/dd/yyyy",
                "M/d/yyyy H:mm", "M/d/yyyy HH:mm",
                "M/d/yyyy H:mm:ss", "M/d/yyyy HH:mm:ss",
                "M/d/yyyy h:mm tt", "M/d/yyyy hh:mm tt",
                "M/d/yyyy h:mm:ss tt", "M/d/yyyy hh:mm:ss tt"
            };
            if (DateTime.TryParseExact(s.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt : null;
        }

        private static PickingListItem? ParseLineItem(string line)
        {
            var m = RowRx.Match(line);
            if (!m.Success) return null;

            var item = new PickingListItem
            {
                LineNumber = int.Parse(m.Groups["LineNumber"].Value, CultureInfo.InvariantCulture),
                Quantity = ParseDec(m.Groups["Quantity"].Value) ?? 0,
                Unit = m.Groups["Unit"].Value.ToUpperInvariant(),
                ItemId = m.Groups["ItemId"].Value,
                ItemDescription = m.Groups["ItemDescription"].Value.Trim(),
                Width = ParseDec(m.Groups["Width"].Value),
                Length = string.IsNullOrWhiteSpace(m.Groups["Length"].Value) ? (decimal?)null : ParseDec(m.Groups["Length"].Value),
                Weight = ParseDec(m.Groups["Weight"].Value)
            };

            item.NeedsAttention = (item.Quantity <= 0) || (item.Width is null or <= 0) || (item.Weight is null or <= 0);
            return item;
        }

        private static decimal? ParseDec(string s)
        {
            var cleaned = s.Replace("\"", "").Replace(",", "").Trim();
            return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                ? Math.Round(d, 3)
                : (decimal?)null;
        }
    }
}
