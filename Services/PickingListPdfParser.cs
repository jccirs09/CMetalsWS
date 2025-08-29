using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
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

            // Order date and ship date
            var printDt = MatchFirst(lines, @"PRINT\s+DATE/TIME:\s*(?<dt>.+)", "dt");
            var shipDt = MatchFirst(lines, @"SHIP\s+DATE\s*(?<dt>\d{2}/\d{2}/\d{4})", "dt");
            pl.OrderDate = ParseDateTime(printDt) ?? DateTime.UtcNow;
            pl.ShipDate = ParseDate(shipDt);

            // Customer name
            var soldToIdx = IndexOf(lines, l => Regex.IsMatch(l, @"\bSOLD TO\b", RegexOptions.IgnoreCase));
            if (soldToIdx >= 0)
            {
                // In the new format, the name is on the same line or the one after.
                var match = Regex.Match(lines[soldToIdx], @"SOLD TO\s+(?<name>.+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    pl.CustomerName = match.Groups["name"].Value;
                else
                    pl.CustomerName = SafeGet(lines, soldToIdx + 1);
            }

            // Ship-to address
            var shipToIdx = IndexOf(lines, l => Regex.IsMatch(l, @"\bSHIP TO\b", RegexOptions.IgnoreCase));
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
                if(shipToLines.Any())
                    pl.ShipToAddress = Clean(string.Join(", ", shipToLines));
            }

            // Ship via: primary signal (header) + inline variant near SHIP TO (e.g., "OUR TRUCK")
            string? shipViaHeader = null, shipViaInline = null;

            var shipViaIdx = IndexOf(lines, l => Regex.IsMatch(l, @"\bSHIP\s+VIA\b", RegexOptions.IgnoreCase));
            if (shipViaIdx >= 0)
            {
                var block = string.Join(" ", lines.Skip(shipViaIdx).Take(4));
                shipViaHeader = Clean(MatchFirst(new[] { block }, @"\bSHIP\s+VIA\b\s*(?<via>[A-Za-z0-9 \-_/]+)", "via"));
            }

            if (shipToIdx >= 0)
            {
                var area = string.Join(" ", lines.Skip(shipToIdx).Take(5));
                // Capture short tokens commonly printed inline beside SHIP TO
                shipViaInline = MatchFirst(new[] { area }, @"\b(OUR\s+TRUCK|TRUCK|COURIER|PICKUP)\b", "0");
                if (!string.IsNullOrWhiteSpace(shipViaInline))
                    shipViaInline = shipViaInline.ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(shipViaHeader) && !string.IsNullOrWhiteSpace(shipViaInline) && !shipViaHeader!.Contains(shipViaInline!, StringComparison.OrdinalIgnoreCase))
                pl.ShippingMethod = $"{shipViaHeader} ({shipViaInline})";
            else
                pl.ShippingMethod = shipViaHeader ?? shipViaInline;

            // Items
            ParseItems(lines, pl.Items);

            // Final fallback: if customer name is blank, lift from first Ship-To line
            if (string.IsNullOrWhiteSpace(pl.CustomerName) && shipToIdx >= 0)
            {
                var alt = SafeGet(lines, shipToIdx + 1);
                if (!string.IsNullOrWhiteSpace(alt))
                    pl.CustomerName = alt;
            }

            return pl;
        }

        private static void ParseItems(List<string> lines, ICollection<PickingListItem> items)
        {
            var header = IndexOf(lines, l =>
                l.Contains("LINE") && l.Contains("QUANTITY") && l.Contains("DESCRIPTION"));
            if (header < 0) return;

            var i = header + 1;
            while (i < lines.Count)
            {
                var raw = lines[i];
                if (string.IsNullOrWhiteSpace(raw) || IsHeaderOrFooter(raw) || !char.IsDigit(raw[0]))
                {
                    i++;
                    continue;
                }

                // Flexible pattern for the start of a line item
                var m = Regex.Match(raw, @"^(?<line>\d+)\s+(?<qty>[\d,]+)\s+(?<unit>LBS|PCS|EA)\b", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    var lineNum = ToInt(m.Groups["line"].Value) ?? 0;
                    var qty = ToDec(m.Groups["qty"].Value) ?? 0m;
                    var unit = m.Groups["unit"].Value.ToUpperInvariant();

                    // The rest of the line is part of the description block
                    var remainingLine = raw.Substring(m.Length).Trim();

                    // The values at the end of the line are width and weight
                    var endValues = Regex.Match(raw, @"(?<width>[\d\.,]+)"")?\s+(?<weight>[\d,]+)$");
                    var width = ToDec(endValues.Groups["width"].Value);
                    var weight = ToDec(endValues.Groups["weight"].Value);

                    var descriptionBlock = new StringBuilder(remainingLine);
                    var descLines = 0;
                    for (var j = i + 1; j < lines.Count; j++)
                    {
                        var nextLine = lines[j];
                        if (string.IsNullOrWhiteSpace(nextLine) || IsHeaderOrFooter(nextLine) || (char.IsDigit(nextLine[0]) && Regex.IsMatch(nextLine, @"^\d+\s")))
                        {
                            break;
                        }
                        descriptionBlock.AppendLine(nextLine.Trim());
                        descLines++;
                    }

                    var fullDescription = Clean(descriptionBlock.ToString());
                    var descWords = fullDescription.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var itemId = descWords.FirstOrDefault() ?? string.Empty;

                    var pli = new PickingListItem
                    {
                        LineNumber = lineNum,
                        Quantity = qty,
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

        private static int _descLinesConsumed = 0;

        private static string ExtractDescription(List<string> lines, int startIdx)
        {
            var sb = new StringBuilder();
            var idx = startIdx;

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
                foreach (var word in page.GetWords())
                {
                    sb.Append(word.Text);
                    sb.Append(' ');
                }
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