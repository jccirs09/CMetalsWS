using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CMetalsWS.Data;
using UglyToad.PdfPig;

namespace CMetalsWS.Services
{
    public class PickingListPdfParser : IPickingListPdfParser
    {
        public PickingList Parse(Stream pdfStream, int branchId, int? customerId = null, int? truckId = null)
        {
            var text = ExtractText(pdfStream);

            // --- DEBUGGING: Return raw text ---
            var debugPl = new PickingList
            {
                SalesOrderNumber = "DEBUG",
                CustomerName = "Raw PdfPig Text ->",
                ShipToAddress = "See Item Description below"
            };
            debugPl.Items.Add(new PickingListItem
            {
                ItemDescription = text
            });
            return debugPl;
            // --- END DEBUGGING ---
        }

        private static void ParseItems(List<string> lines, ICollection<PickingListItem> items)
        {
            var headerIdx = IndexOf(lines, l => l.StartsWith("LINE", StringComparison.OrdinalIgnoreCase) && l.Contains("DESCRIPTION"));
            if (headerIdx < 0) return;

            var itemStartRegex = new Regex(@"^(?<line>\d+)\s+(?<qty>[\d,]+)\s+PCS");

            for (int i = headerIdx + 1; i < lines.Count; i++)
            {
                var line = lines[i];
                if (IsHeaderOrFooter(line)) break;

                var match = itemStartRegex.Match(line);
                if (!match.Success) continue;

                // We found the start of an item.
                var item = new PickingListItem
                {
                    LineNumber = ToInt(match.Groups["line"].Value) ?? 0,
                    Quantity = ToDec(match.Groups["qty"].Value) ?? 0,
                    Unit = "PCS"
                };

                // The rest of the line contains ItemId and possibly dimensions
                var remainder = line.Substring(match.Length).Trim();
                var parts = Regex.Split(remainder, @"\s{2,}"); // Split by 2+ spaces

                item.ItemId = parts.FirstOrDefault() ?? "";

                // Dimensions are tricky. Let's look for numbers with " at the end.
                var sizeParts = remainder.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                item.Width = ToDec(sizeParts.FirstOrDefault(p => p.EndsWith("\""))?.TrimEnd('"'));
                item.Length = ToDec(sizeParts.Skip(1).FirstOrDefault(p => p.EndsWith("\""))?.TrimEnd('"'));
                item.Weight = ToDec(sizeParts.LastOrDefault(p => decimal.TryParse(p, out _)));


                // Now, read subsequent lines that belong to this item
                var descriptionLines = new List<string>();
                i++;
                while (i < lines.Count && !itemStartRegex.IsMatch(lines[i]) && !IsHeaderOrFooter(lines[i]))
                {
                    descriptionLines.Add(lines[i].Trim());
                    i++;
                }
                // We've either hit the next item or the footer, so decrement i to account for the loop's increment
                if (i < lines.Count) i--;

                item.ItemDescription = string.Join(" ", descriptionLines.Where(l => !string.IsNullOrWhiteSpace(l)));
                items.Add(item);
            }
        }

        private static string ExtractText(Stream pdfStream)
        {
            if (pdfStream.CanSeek) pdfStream.Position = 0;

            var text = new StringBuilder();
            using (var pdf = PdfDocument.Open(pdfStream))
            {
                foreach (var page in pdf.GetPages())
                {
                    text.AppendLine(page.Text);
                }
            }
            return text.ToString();
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

        private static string? GetValueFromNextLine(List<string> lines, string label)
        {
            var idx = IndexOf(lines, l => l.Contains(label, StringComparison.OrdinalIgnoreCase));
            if (idx == -1) return null;
            return NextNonEmpty(lines, idx + 1);
        }

        private static string? SafeGet(List<string> lines, int index) =>
            (index >= 0 && index < lines.Count) ? lines[index] : null;

        private static string NextNonEmpty(List<string> lines, int start)
        {
            if (start >= lines.Count) return string.Empty;
            return lines.Skip(start).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l)) ?? string.Empty;
        }

        private static string NextNonEmptyBlock(List<string> lines, int start, int maxLines, Func<string, bool>? stopIf = null)
        {
            var blockLines = lines.Skip(start)
                                  .TakeWhile(l => stopIf == null || !stopIf(l))
                                  .Where(l => !string.IsNullOrWhiteSpace(l))
                                  .Take(maxLines);
            return string.Join(", ", blockLines);
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
