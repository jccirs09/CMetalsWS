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

        private static readonly string[] NewlineSplit = new[] { "\r\n", "\n" };
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

            var salesOrderNumber = ExtractFirst(fullText, @"PICKING\s*LIST\s*No\.?\s*(\d+)")
                ?? ExtractFirst(fullText, @"\b\d{7,}\b")
                ?? throw new InvalidOperationException("Could not parse Sales Order Number.");

            var pickingList = new PickingList
            {
                SalesOrderNumber = salesOrderNumber,
                SourceFileName = sourceFileName,
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
            pickingList.PrintDateTime = ParseDateFlexible(
                ExtractFirst(fullText, @"(?im)PRINT\s*DATE/TIME:\s*(.+)$")
                ?? ExtractFirst(fullText, @"(?im)PRINT\s*DATE\/TIME\s*:\s*(.+)$"));

            pickingList.ShipDate  = ParseDateFlexible(ExtractFirst(fullText, @"(?im)SHIP\s*DATE[:\s]\s*(\d{1,2}/\d{1,2}/\d{4})")
                                ?? ExtractLabelInlineOrNextLine(fullText, "SHIP DATE"));

            pickingList.OrderDate = ParseDateFlexible(ExtractFirst(fullText, @"(?im)ORDER\s*DATE[:\s]?\s*(\d{1,2}/\d{1,2}/\d{4})")
                                ?? ExtractLabelInlineOrNextLine(fullText, "ORDER DATE"));

            pickingList.Buyer       = ExtractLabelInlineOrNextLine(fullText, "BUYER");
            pickingList.SalesRep    = ExtractLabelInlineOrNextLine(fullText, "SALES REP");
            pickingList.ShippingVia = ExtractLabelInlineOrNextLine(fullText, "SHIP VIA");
            pickingList.FOB         = ExtractLabelInlineOrNextLine(fullText, "FOB POINT");

            pickingList.SoldTo = ExtractBlockBetweenLabels(fullText, "SOLD TO", "SHIP TO|SHIP VIA|FOB POINT|TERMS|LINE");
            pickingList.ShipTo = ExtractBlockBetweenLabels(fullText, "SHIP TO", "SHIP VIA|FOB POINT|TERMS|LINE");

            pickingList.ParseNotes = string.Join("\n",
                fullText.Split(NewlineSplit, StringSplitOptions.None)
                        .Select(s => s.Trim())
                        .Where(s => s.StartsWith("MAX SKID WEIGHT", StringComparison.OrdinalIgnoreCase)
                                    || s.StartsWith("RECEIVING HOURS", StringComparison.OrdinalIgnoreCase)));

            // Line item parsing
            var lines = fullText.Split(NewlineSplit, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                var item = ParseLineItem(lines[i]);
                if (item == null) continue;

                var notes = new List<string>();
                for (int j = i + 1; j < lines.Length; j++)
                {
                    var next = lines[j].Trim();
                    if (RowRx.IsMatch(next)
                        || next.StartsWith("SOURCE:", StringComparison.OrdinalIgnoreCase)
                        || next.StartsWith("TAG #", StringComparison.OrdinalIgnoreCase)
                        || next.StartsWith("Other Reservations:", StringComparison.OrdinalIgnoreCase)
                        || next.StartsWith("PULLED BY", StringComparison.OrdinalIgnoreCase))
                    {
                        if (next.Contains("TAG #") && next.Contains("HEAT #") && next.Contains("MILL REF #"))
                            item.HasTagLots = true;
                        break;
                    }
                    notes.Add(next);
                }
                item.SalesNote = string.Join("\n", notes);
                pickingList.Items.Add(item);
            }

            pickingList.HasParseIssues = pickingList.Items.Any(i => i.NeedsAttention);

            return await Task.FromResult(pickingList);
        }

        private PickingListItem? ParseLineItem(string line)
        {
            var m = RowRx.Match(line);
            if (!m.Success) return null;

            var item = new PickingListItem
            {
                LineNumber = int.Parse(m.Groups["LineNumber"].Value, CultureInfo.InvariantCulture),
                Quantity   = ParseDec(m.Groups["Quantity"].Value) ?? 0,
                Unit       = m.Groups["Unit"].Value.ToUpperInvariant(),
                ItemId     = m.Groups["ItemId"].Value,
                ItemDescription = m.Groups["ItemDescription"].Value.Trim(),
                Width      = ParseDec(m.Groups["Width"].Value),
                Length     = string.IsNullOrWhiteSpace(m.Groups["Length"].Value) ? null : ParseDec(m.Groups["Length"].Value),
                Weight     = ParseDec(m.Groups["Weight"].Value),
            };

            item.NeedsAttention = (item.Quantity <= 0) || (item.Width is null or <= 0) || (item.Weight is null or <= 0);
            return item;

            static decimal? ParseDec(string s)
            {
                var cleaned = s.Replace("\"", "").Replace(",", "").Trim();
                return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                    ? Math.Round(d, 3)
                    : (decimal?)null;
            }
        }

        private string? ExtractFirst(string text, string pattern)
        {
            var m = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }

        private string? ExtractLabelInlineOrNextLine(string text, string label)
        {
            var rxInline = new Regex($@"(?im)^\s*{Regex.Escape(label)}\s*:?\s*(?<v>.+?)\s*$");
            var m = rxInline.Match(text);
            if (m.Success) return m.Groups["v"].Value.Trim();

            var lines = text.Split(NewlineSplit, StringSplitOptions.None);
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var hdr = lines[i];
                if (!Regex.IsMatch(hdr, $@"(?i)\b{Regex.Escape(label)}\b")) continue;

                var next = lines[i + 1];
                var hdrCols = Regex.Split(hdr.Trim(), @"\s{2,}");
                var valCols = Regex.Split(next.Trim(), @"\s{2,}");

                for (int c = 0; c < hdrCols.Length; c++)
                {
                    if (Regex.IsMatch(hdrCols[c], $@"(?i)^{Regex.Escape(label)}$"))
                    {
                        if (c < valCols.Length)
                            return valCols[c].Trim();
                    }
                }
            }
            return null;
        }

        private string ExtractBlockBetweenLabels(string text, string startLabel, string endLabelPattern)
        {
            var rxStart = new Regex($@"(?im)^\s*{Regex.Escape(startLabel)}\s*:?\s*$");
            var mStart = rxStart.Match(text);
            if (!mStart.Success) return "";

            var startIdx = mStart.Index + mStart.Length;
            var tail = text.Substring(startIdx);

            var rxEnd = new Regex($@"(?im)^\s*(?:{endLabelPattern})\s*:?\s*$");
            var mEnd = rxEnd.Match(tail);
            var segment = mEnd.Success ? tail.Substring(0, mEnd.Index) : tail;

            var lines = segment.Split(NewlineSplit, StringSplitOptions.None)
                               .Select(s => s.Trim())
                               .Where(s => !string.IsNullOrWhiteSpace(s))
                               .Take(3);
            return string.Join("\n", lines);
        }

        private DateTime? ParseDateFlexible(string? s)
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
            if (DateTime.TryParseExact(s.Trim(), formats, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var dt))
                return dt;

            return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt : null;
        }
    }
}
