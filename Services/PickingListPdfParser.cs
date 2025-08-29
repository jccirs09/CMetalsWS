using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using CMetalsWS.Data;

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

            // Sales order number
            pl.SalesOrderNumber = MatchFirst(lines,
                @"PICKING\s+LIST\s+No\.?\s*(?<num>[0-9A-Za-z\-]+)", "num") ?? string.Empty;

            // Dates
            var printDt = MatchFirst(lines, @"PRINT\s+DATE/TIME:\s*(?<dt>.+)", "dt");
            var shipDt = MatchFirst(lines, @"SHIP\s+DATE\s*(?<dt>\d{2}/\d{2}/\d{4})", "dt");
            pl.OrderDate = ParseDateTime(printDt) ?? DateTime.UtcNow;
            pl.ShipDate = ParseDate(shipDt);

            // Customer name
            var soldToIdx = IndexOf(lines, l => l.StartsWith("SOLD TO", StringComparison.OrdinalIgnoreCase));
            if (soldToIdx >= 0)
                pl.CustomerName = SafeGet(lines, soldToIdx + 1);

            // Ship-to address
            var shipToIdx = IndexOf(lines, l => l.StartsWith("SHIP TO", StringComparison.OrdinalIgnoreCase));
            if (shipToIdx >= 0)
            {
                var shipToLines = new List<string>();
                for (int i = 1; i <= 4; i++)
                {
                    var v = SafeGet(lines, shipToIdx + i);
                    if (string.IsNullOrWhiteSpace(v)) break;
                    if (IsHeaderOrFooter(v)) break;
                    shipToLines.Add(v);
                }
                pl.ShipToAddress = Clean(string.Join(", ", shipToLines));
            }

            // Ship Via (clip at typical breakers to avoid eating the rest of the line)
            var shipViaIdx = IndexOf(lines, l => Regex.IsMatch(l, @"\bSHIP\s+VIA\b", RegexOptions.IgnoreCase));
            if (shipViaIdx >= 0)
            {
                var raw = string.Join(" ", lines.Skip(shipViaIdx).Take(2));
                var via = MatchFirst(new[] { raw }, @"\bSHIP\s+VIA\b\s*(?<via>[A-Za-z0-9 /_\-]+)", "via");
                via = via?.Trim();
                if (!string.IsNullOrEmpty(via))
                {
                    // trim if it accidentally captured following labels
                    via = Regex.Replace(via, @"\b(SOLD\s+TO|SHIP\s+TO|LINE|DESCRIPTION)\b.*$", "", RegexOptions.IgnoreCase).Trim();
                    pl.ShippingMethod = via;
                }
            }

            // Items
            ParseItems(lines, text, pl.Items);

            // Fallback: if customer name blank, lift from ship-to line 1
            if (string.IsNullOrWhiteSpace(pl.CustomerName) && shipToIdx >= 0)
            {
                var alt = SafeGet(lines, shipToIdx + 1);
                if (!string.IsNullOrWhiteSpace(alt))
                    pl.CustomerName = alt;
            }

            return pl;
        }

        private static void ParseItems(List<string> lines, string fullText, ICollection<PickingListItem> items)
        {
            // 1) Try the original line-by-line approach
            var header = IndexOf(lines, l =>
                l.StartsWith("LINE", StringComparison.OrdinalIgnoreCase) &&
                l.Contains("DESCRIPTION", StringComparison.OrdinalIgnoreCase));

            if (header >= 0)
            {
                var i = header + 1;
                while (i < lines.Count)
                {
                    var raw = lines[i];
                    if (string.IsNullOrWhiteSpace(raw) || IsHeaderOrFooter(raw))
                    {
                        i++;
                        continue;
                    }

                    if (raw.StartsWith("CTL -", StringComparison.OrdinalIgnoreCase)) break;
                    if (raw.StartsWith("TAG #", StringComparison.OrdinalIgnoreCase) ||
                        raw.StartsWith("TAG:", StringComparison.OrdinalIgnoreCase) ||
                        raw.StartsWith("SOURCE:", StringComparison.OrdinalIgnoreCase) ||
                        raw.StartsWith("Other Reservations", StringComparison.OrdinalIgnoreCase))
                    {
                        i++;
                        continue;
                    }

                    // A: PCS rows (width + length + weight)
                    var m = Regex.Match(raw,
                        @"^(?<line>\d+)\s+(?<qty>[\d,]+)\s*(?<unit>PCS)\b.*?(?<item>[A-Z0-9\-_\/\.]+)\s+(?<w>\d{1,3}(?:\.\d{1,3})?)""\s+(?<l>\d{1,3}(?:\.\d{1,3})?)""\s+(?<wt>[\d,]+)$",
                        RegexOptions.IgnoreCase);

                    // B: LBS rows (width only, weight at end)
                    if (!m.Success)
                    {
                        m = Regex.Match(raw,
                            @"^(?<line>\d+)\s+(?<qty>[\d,]+)\s*(?<unit>LBS)\b.*?(?<item>[A-Z0-9\-_\/\.]+)\s+(?<w>\d{1,3}(?:\.\d{1,3})?)""\s+(?<wt>[\d,]+)$",
                            RegexOptions.IgnoreCase);
                    }

                    // C: PCS variant
                    if (!m.Success)
                    {
                        m = Regex.Match(raw,
                            @"^(?<line>\d+)\s+(?<qty>[\d,]+)\s*(?<unit>PCS)\b.*?(?<item>[A-Z0-9\-_\/\.]+)\s+(?<w>\d{1,3}(?:\.\d{1,3})?)""\s+(?<l>\d{1,3}(?:\.\d{1,3})?)""\s+(?<wt>\d{1,3}(?:,\d{3})*(?:\.\d+)?)$",
                            RegexOptions.IgnoreCase);
                    }

                    if (m.Success)
                    {
                        items.Add(BuildItemFromMatch(m, lines, i + 1));
                        i += _descLinesConsumed;
                        _descLinesConsumed = 0;
                        i++;
                        continue;
                    }

                    i++;
                }
            }

            // 2) If nothing found (your screenshot case), use a multiline fallback over the items block.
            if (items.Count == 0)
            {
                // Take text between header row and a terminating marker
                var block = ExtractItemsBlock(fullText);

                // Pattern for LBS (no length)
                var lbs = Regex.Matches(block,
                    @"(?m)^\s*(?<line>\d+)\s+(?<qty>[\d,]+)\s*(?<unit>LBS)\b.*?(?<item>[A-Z0-9\-/\.]+)\s+(?<w>\d{1,3}(?:\.\d{1,3})?)""\s+(?<wt>[\d,]+)\s*$",
                    RegexOptions.IgnoreCase);

                foreach (Match m in lbs.Cast<Match>())
                    items.Add(BuildItemFromMatch(m, null, -1));

                // Pattern for PCS (has length)
                var pcs = Regex.Matches(block,
                    @"(?m)^\s*(?<line>\d+)\s+(?<qty>[\d,]+)\s*(?<unit>PCS)\b.*?(?<item>[A-Z0-9\-/\.]+)\s+(?<w>\d{1,3}(?:\.\d{1,3})?)""\s+(?<l>\d{1,3}(?:\.\d{1,3})?)""\s+(?<wt>[\d,]+)\s*$",
                    RegexOptions.IgnoreCase);

                foreach (Match m in pcs.Cast<Match>())
                    items.Add(BuildItemFromMatch(m, null, -1));
            }
        }

        private static PickingListItem BuildItemFromMatch(Match m, List<string>? lines, int descStart)
        {
            var lineNum = ToInt(m.Groups["line"].Value) ?? 0;
            var qty = ToDec(m.Groups["qty"].Value) ?? 0m;
            var unit = m.Groups["unit"].Value.ToUpperInvariant();
            var itemId = Clean(m.Groups["item"].Value);
            var width = ToDec(m.Groups["w"].Value);
            decimal? length = null;
            if (m.Groups["l"]?.Success == true)
                length = ToDec(m.Groups["l"].Value);
            var weight = ToDec(m.Groups["wt"].Value);

            string description = itemId;
            if (lines != null && descStart >= 0)
            {
                description = ExtractDescription(lines, descStart);
            }

            return new PickingListItem
            {
                LineNumber = lineNum,
                Quantity = qty,
                Unit = unit == "LBS" ? "LBS" : "EA",
                ItemId = itemId,
                ItemDescription = description,
                Width = width,
                Length = length,
                Weight = weight
            };
        }

        private static string ExtractItemsBlock(string fullText)
        {
            // Find the header "LINE ... DESCRIPTION" and cut until a typical trailer line
            var start = Regex.Match(fullText, @"LINE\s+QTY.*DESCRIPTION", RegexOptions.IgnoreCase);
            if (!start.Success) return fullText;

            var tail = Regex.Match(fullText, @"(?:(?:PULLED BY)|(?:TERMS)|(?:MAX SKID WEIGHT)|(?:MAX COIL WEIGHT))", RegexOptions.IgnoreCase);
            if (tail.Success && tail.Index > start.Index)
                return fullText.Substring(start.Index, tail.Index - start.Index);

            return fullText.Substring(start.Index);
        }

        private static int _descLinesConsumed = 0;

        private static string ExtractDescription(List<string> lines, int startIdx)
        {
            var sb = new StringBuilder();
            int idx = startIdx;

            while (idx < lines.Count)
            {
                var s = lines[idx];
                if (string.IsNullOrWhiteSpace(s)) { idx++; continue; }
                if (IsHeaderOrFooter(s)) break;
                if (Regex.IsMatch(s, @"^\d+\s+[\d,]+\s*(PCS|LBS)\b", RegexOptions.IgnoreCase)) break;
                if (s.StartsWith("TAG", StringComparison.OrdinalIgnoreCase) ||
                    s.StartsWith("SOURCE:", StringComparison.OrdinalIgnoreCase) ||
                    s.StartsWith("Other Reservations", StringComparison.OrdinalIgnoreCase) ||
                    s.StartsWith("TAG #", StringComparison.OrdinalIgnoreCase) ||
                    s.StartsWith("LOC", StringComparison.OrdinalIgnoreCase))
                    break;

                sb.Append(' ');
                sb.Append(s.Trim());
                idx++;
            }

            _descLinesConsumed = Math.Max(0, idx - startIdx);
            return Clean(sb.ToString());
        }

        private static bool IsHeaderOrFooter(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (s.StartsWith("LINE", StringComparison.OrdinalIgnoreCase) && s.Contains("DESCRIPTION")) return true;
            if (s.Contains("QTY STAGED", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("PICKING LIST No.", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("PRINT DATE/TIME", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("PULLED BY TOTAL WT", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("TERMS", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Contains("MAX SKID WEIGHT", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Contains("MAX COIL WEIGHT", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("----------------", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("********************************", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("================================", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Equals("CTL", StringComparison.OrdinalIgnoreCase) || s.Equals("- CTL -", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Equals("- SHEET STOCK -", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static string ExtractText(Stream pdfStream)
        {
            var sb = new StringBuilder();
            using var doc = PdfDocument.Open(pdfStream);
            foreach (var page in doc.GetPages())
            {
                // Content-order extraction preserves natural line breaks much better
                var pageText = ContentOrderTextExtractor.GetText(page);
                sb.AppendLine(pageText);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static List<string> SplitLines(string text)
        {
            return text
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();
        }

        private static string? MatchFirst(IEnumerable<string> lines, string pattern, string groupName)
        {
            foreach (var l in lines)
            {
                var m = Regex.Match(l, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    if (groupName == "0") return m.Groups[0].Value.Trim();
                    return m.Groups[groupName].Value.Trim();
                }
            }
            return null;
        }

        private static int IndexOf(List<string> lines, Func<string, bool> pred)
        {
            for (int i = 0; i < lines.Count; i++)
                if (pred(lines[i])) return i;
            return -1;
        }

        private static string? SafeGet(List<string> lines, int index)
        {
            if (index >= 0 && index < lines.Count) return lines[index];
            return null;
        }

        private static DateTime? ParseDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParseExact(s.Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            return null;
        }

        private static DateTime? ParseDateTime(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (DateTime.TryParseExact(string.Join(' ', parts.Take(2)), "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            if (DateTime.TryParse(s, out dt)) return dt;
            return null;
        }

        private static int? ToInt(string s)
        {
            if (int.TryParse(s.Replace(",", ""), out var v)) return v;
            return null;
        }

        private static decimal? ToDec(string s)
        {
            if (decimal.TryParse(s.Replace(",", "").Replace("\"", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            return null;
        }

        private static string Clean(string s) => Regex.Replace(s ?? string.Empty, @"\s+", " ").Trim();
    }
}
