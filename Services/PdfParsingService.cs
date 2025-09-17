using CMetalsWS.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;

namespace CMetalsWS.Services
{
    public class PdfParsingService : IPdfParsingService
    {
        private readonly ILogger<PdfParsingService> _logger;
        public PdfParsingService(ILogger<PdfParsingService> logger) => _logger = logger;

        public Task<(PickingList, List<PickingListItem>)> ParsePickingListAsync(byte[] pdfBytes)
        {
            using var doc = PdfDocument.Open(pdfBytes);

            var header = ParseHeader(doc);
            var items = ParseLineItems(doc);

            if (header.TotalWeight == 0 && items.Any(i => i.Weight.HasValue))
                header.TotalWeight = items.Where(i => i.Weight.HasValue).Sum(i => i.Weight!.Value);

            return Task.FromResult((header, items));
        }

        // -------------------- tiny helpers --------------------
        private static string NormToken(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = s.Trim();
            if (s.EndsWith(":", StringComparison.Ordinal)) s = s.Substring(0, s.Length - 1);
            return s.ToUpperInvariant();
        }

        private static bool HasAnyLetter(string s)
        {
            for (int i = 0; i < s.Length; i++) if (char.IsLetter(s[i])) return true;
            return false;
        }

        private static decimal? ParseDecimalLoose(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if ((c >= '0' && c <= '9') || c == '.' || c == '-') sb.Append(c);
            }
            if (sb.Length == 0) return null;
            if (decimal.TryParse(sb.ToString(),
                                 NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                                 CultureInfo.InvariantCulture,
                                 out var d))
                return d;
            return null;
        }

        private static DateTime? ParseDateLoose(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out var d)) return d;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;
            return null;
        }

        private static string JoinRaw(IEnumerable<string> parts)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var p in parts)
            {
                if (string.IsNullOrEmpty(p)) continue;
                if (!first) sb.Append(' ');
                sb.Append(p);
                first = false;
            }
            return sb.ToString();
        }

        // return multi-line address text
        private static string CombineAddress(string sameLine, string below)
        {
            if (string.IsNullOrWhiteSpace(sameLine) && string.IsNullOrWhiteSpace(below)) return null;
            if (string.IsNullOrWhiteSpace(sameLine)) return below.Trim();
            if (string.IsNullOrWhiteSpace(below)) return sameLine.Trim();
            return $"{sameLine.Trim()}\n{below.Trim()}";
        }

        // -------------------- page → lines model --------------------
        private sealed class W
        {
            public Word Word;
            public string Raw;
            public string Norm;
            public double X;
            public double Y;
            public PdfRectangle Box;
        }

        private sealed class L
        {
            public List<W> Words;  // sorted by X asc
            public double Y;
            private string[] _normTokensCache;

            public L(List<W> words, double y) { Words = words; Y = y; }
            public string RawLineText => JoinRaw(Words.Select(w => w.Raw));

            public string[] NormTokens
            {
                get
                {
                    if (_normTokensCache == null) _normTokensCache = Words.Select(w => w.Norm).ToArray();
                    return _normTokensCache;
                }
            }
        }

        /// Lines are sorted top→bottom (PDF Y desc). Larger index == lower on page.
        private static List<L> GetLines(Page page, double yTol = 1.6)
        {
            var words = page.GetWords().ToList();
            var list = new List<W>(Math.Max(16, words.Count));
            foreach (var w in words)
            {
                var b = w.BoundingBox;
                var c = b.Centroid;
                list.Add(new W { Word = w, Raw = w.Text, Norm = NormToken(w.Text), X = c.X, Y = c.Y, Box = b });
            }

            return list.GroupBy(w => Math.Round(w.Y / yTol) * yTol)
                       .OrderByDescending(g => g.Key)
                       .Select(g => new L(g.OrderBy(x => x.X).ToList(), g.Key))
                       .ToList();
        }

        // -------------------- label helpers --------------------
        private static readonly string[] LabelPhrases =
        {
            "SOLD TO","SHIP TO","SALES REP","SALESPERSON","SHIP VIA","SHIPPING VIA",
            "ORDER DATE","SHIP DATE","FOB","FOB POINT","TOTAL WEIGHT","TOTAL WT",
            "PRINT DATE/TIME","PRINT DATE","PRINTED","BUYER","PICKING GROUP","ROUTE","MILL CERTS"
        };

        private static readonly HashSet<string> StopTokens = new(StringComparer.Ordinal)
        {
            "LINE","QUANTITY","QTY","QTY STAGED","DESCRIPTION","ITEM","WIDTH","LENGTH","WEIGHT","UNIT","UOM",
            "RECEIVING","HOURS","DELIVERED","TERMS","MAX","SKID","ROUTE","MILL","CERTS",
            "SOURCE","TAG","HEAT","PULLED","BY","TOTAL","WT",
            "SOLD","TO","SHIP","VIA","SALES","REP","ORDER","DATE","FOB","PRINT","PRINTED","BUYER","PURCHASE"
        };

        private static bool TryFindPhraseOnLine(L line, string phrase, out PdfRectangle box, out int startIdx)
        {
            startIdx = -1;
            box = default;
            var parts = phrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(NormToken).ToArray();
            if (parts.Length == 0) return false;

            var toks = line.NormTokens;
            for (int i = 0; i <= toks.Length - parts.Length; i++)
            {
                bool ok = true;
                for (int k = 0; k < parts.Length; k++)
                    if (!toks[i + k].Equals(parts[k], StringComparison.Ordinal)) { ok = false; break; }

                if (!ok) continue;

                var b = line.Words[i].Box;
                for (int k = 1; k < parts.Length; k++)
                {
                    var nb = line.Words[i + k].Box;
                    b = new PdfRectangle(Math.Min(b.Left, nb.Left), Math.Min(b.Bottom, nb.Bottom),
                                         Math.Max(b.Right, nb.Right), Math.Max(b.Top, nb.Top));
                }
                startIdx = i; box = b; return true;
            }
            return false;
        }

        private static double? NextLabelRightOnSameLine(L line, PdfRectangle currentLabelBox)
        {
            double? best = null;
            foreach (var phrase in LabelPhrases)
            {
                if (TryFindPhraseOnLine(line, phrase, out var b, out _))
                {
                    if (b.Left > currentLabelBox.Right + 0.5)
                        if (best == null || b.Left < best.Value) best = b.Left;
                }
            }
            return best;
        }

        private static string ReadRightSameLineClamped(L line, PdfRectangle labelBox, double defaultMaxWidth = 360)
        {
            var stopX = NextLabelRightOnSameLine(line, labelBox);
            double right = stopX.HasValue ? stopX.Value - 0.75 : (labelBox.Left + defaultMaxWidth);
            var seq = line.Words.Where(w => w.Box.Left > labelBox.Right + 0.4 && w.X <= right)
                                .Select(w => w.Raw);
            return JoinRaw(seq);
        }

        /// Read up to maxLines below label (stopping at labels/table header). Returns multi-line string.
        private static string BlockBelowClamped(List<L> lines, int labelLineIndex, PdfRectangle labelBox,
                                               int maxLines, int tableHeaderIndex,
                                               double leftPad = -2, double defaultRight = 420)
        {
            if (labelLineIndex < 0 || labelLineIndex >= lines.Count) return null;

            double left = labelBox.Left + leftPad;
            double right = labelBox.Left + defaultRight;

            var nextX = NextLabelRightOnSameLine(lines[labelLineIndex], labelBox);
            if (nextX.HasValue) right = Math.Min(right, nextX.Value - 0.75);

            var kept = new List<string>(maxLines);

            for (int i = labelLineIndex + 1; i < lines.Count && kept.Count < maxLines; i++)
            {
                if (tableHeaderIndex >= 0 && i >= tableHeaderIndex) break;

                var line = lines[i];

                foreach (var phrase in LabelPhrases)
                    if (TryFindPhraseOnLine(line, phrase, out _, out _)) goto Done;

                var seg = line.Words.Where(w => w.X >= left && w.X <= right).Select(w => w.Raw);
                var text = JoinRaw(seg).Trim();
                if (text.Length == 0) continue;

                if (line.NormTokens.Any(t => StopTokens.Contains(t))) break;

                kept.Add(text);
            }
        Done:
            if (kept.Count == 0) return null;
            return string.Join("\n", kept);         // <<< multi-line return
        }

        // -------------------- table header helpers --------------------
        private enum Col { Line, Qty, QtyStaged, Description, Width, Length, Weight, Unit }

        private static readonly string[][] HeaderAliases =
        {
            new [] { "LINE" },
            new [] { "QUANTITY", "QTY" },
            new [] { "QTY STAGED", "QTY_STAGED", "STAGED" },
            new [] { "DESCRIPTION", "ITEM", "ITEM DESCRIPTION" },
            new [] { "WIDTH" },
            new [] { "LENGTH" },
            new [] { "WEIGHT", "WT", "LBS" },
            new [] { "UNIT", "UOM" }
        };

        private static int FindTableHeaderIndex(List<L> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var t = lines[i].NormTokens;
                bool hasLine = false, hasDesc = false;
                for (int k = 0; k < t.Length; k++)
                {
                    if (t[k] == "LINE") hasLine = true;
                    if (t[k] == "DESCRIPTION" || t[k] == "ITEM") hasDesc = true;
                }
                if (hasLine && hasDesc) return i;
            }
            return -1;
        }

        // -------------------- header parsing --------------------
        private PickingList ParseHeader(PdfDocument doc)
        {
            var page = doc.GetPage(1);
            var lines = GetLines(page);
            var tableHeaderIdx = FindTableHeaderIndex(lines);

            // Prefer the more specific label first
            var soldToLbl = FindLabelBox(lines, "SOLD TO");
            var shipToLbl = FindLabelBox(lines, "SHIP TO");
            var fobLbl = FindLabelBox(lines, "FOB POINT", "FOB");
            var ordLbl = FindLabelBox(lines, "ORDER DATE");
            var shipDateLbl = FindLabelBox(lines, "SHIP DATE");
            var salesLbl = FindLabelBox(lines, "SALES REP", "SALESPERSON");
            var shipViaLbl = FindLabelBox(lines, "SHIP VIA", "SHIPPING VIA");
            var totWtLbl = FindLabelBox(lines, "TOTAL WEIGHT", "TOTAL WT");
            var printLbl = FindLabelBox(lines, "PRINT DATE/TIME", "PRINT DATE", "PRINTED");

            string ReadSameOrBelow((int lineIndex, PdfRectangle box)? lbl, double maxW = 320)
            {
                if (lbl == null) return null;
                var same = ReadRightSameLineClamped(lines[lbl.Value.lineIndex], lbl.Value.box, maxW);
                if (!string.IsNullOrWhiteSpace(same)) return same.Trim();
                var below = BlockBelowClamped(lines, lbl.Value.lineIndex, lbl.Value.box, 1, tableHeaderIdx, -2, maxW);
                return string.IsNullOrWhiteSpace(below) ? null : below.Trim();
            }

            // Addresses
            string soldTo = null;
            if (soldToLbl != null)
            {
                var same = ReadRightSameLineClamped(lines[soldToLbl.Value.lineIndex], soldToLbl.Value.box, 420);
                var below = BlockBelowClamped(lines, soldToLbl.Value.lineIndex, soldToLbl.Value.box, 5, tableHeaderIdx, -2, 420);
                soldTo = CombineAddress(same, below);
            }

            string shipTo = null;
            if (shipToLbl != null)
            {
                var same = ReadRightSameLineClamped(lines[shipToLbl.Value.lineIndex], shipToLbl.Value.box, 420);
                var below = BlockBelowClamped(lines, shipToLbl.Value.lineIndex, shipToLbl.Value.box, 5, tableHeaderIdx, -2, 420);
                shipTo = CombineAddress(same, below);
            }

            // Sales rep / ship via / dates
            var salesRep = ReadSameOrBelow(salesLbl);
            var shipVia = ReadSameOrBelow(shipViaLbl);
            var orderDateText = ReadSameOrBelow(ordLbl, 260);
            var shipDateText = ReadSameOrBelow(shipDateLbl, 260);

            // FOB (prefer below)
            string fob = null;
            if (fobLbl != null)
            {
                fob = BlockBelowClamped(lines, fobLbl.Value.lineIndex, fobLbl.Value.box, 1, tableHeaderIdx, -2, 260)
                      ?? ReadRightSameLineClamped(lines[fobLbl.Value.lineIndex], fobLbl.Value.box, 260);
                if (!string.IsNullOrWhiteSpace(fob)) fob = fob.Trim();
            }

            // Print and total
            string print = ReadSameOrBelow(printLbl, 260);

            decimal totalWeight = 0m;
            if (totWtLbl != null)
            {
                var s = ReadSameOrBelow(totWtLbl, 220);
                if (!string.IsNullOrWhiteSpace(s)) totalWeight = ParseDecimalLoose(s) ?? 0m;
            }

            var list = new PickingList
            {
                SalesOrderNumber = ExtractPickingListNumberFast(lines) ?? string.Empty,
                OrderDate = ParseDateLoose(orderDateText ?? string.Empty),
                ShipDate = ParseDateLoose(shipDateText ?? string.Empty),
                SoldTo = string.IsNullOrWhiteSpace(soldTo) ? null : soldTo.Trim(),
                ShipTo = string.IsNullOrWhiteSpace(shipTo) ? null : shipTo.Trim(),
                SalesRep = string.IsNullOrWhiteSpace(salesRep) ? null : salesRep.Trim(),
                ShippingVia = string.IsNullOrWhiteSpace(shipVia) ? null : shipVia.Trim(),
                FOB = string.IsNullOrWhiteSpace(fob) ? null : fob.Trim(),
                Buyer = null,
                PrintDateTime = ParseDateLoose(print ?? string.Empty),
                TotalWeight = totalWeight
            };

            if (string.IsNullOrWhiteSpace(list.SalesOrderNumber))
            {
                var top = string.Join(" ", lines.Take(6).Select(l => l.RawLineText));
                var m = Regex.Match(top, @"\b\d{6,}\b");
                if (m.Success) list.SalesOrderNumber = m.Value;
            }

            return list;
        }

        private static (int lineIndex, PdfRectangle box)? FindLabelBox(List<L> lines, params string[] aliases)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                foreach (var alias in aliases)
                {
                    if (TryFindPhraseOnLine(lines[i], alias, out var b, out _))
                        return (i, b);
                }
            }
            return null;
        }

        private static string ExtractPickingListNumberFast(List<L> lines)
        {
            var label = FindLabelBox(lines,
                "PICKING LIST NO", "PICKING LIST #", "PICKING LIST NUMBER",
                "PICK LIST NO", "PICK LIST #", "P/L NO", "PL NO")
                ?? FindLabelBox(lines, "PICKING LIST");

            if (label != null)
            {
                var same = ReadRightSameLineClamped(lines[label.Value.lineIndex], label.Value.box, 300);
                if (!string.IsNullOrWhiteSpace(same))
                {
                    var m1 = Regex.Match(same, @"\b\d{5,}\b");
                    if (m1.Success) return m1.Value;
                }

                var below = BlockBelowClamped(lines, label.Value.lineIndex, label.Value.box, 1, int.MaxValue, -2, 300);
                if (!string.IsNullOrWhiteSpace(below))
                {
                    var m2 = Regex.Match(below, @"\b\d{5,}\b");
                    if (m2.Success) return m2.Value;
                }
            }
            return null;
        }

        // -------------------- table parsing --------------------
       

        private sealed class Spans
        {
            public readonly Dictionary<Col, (double left, double right)> Map =
                new Dictionary<Col, (double left, double right)>();
        }

        private static Spans BuildSpansFromHeader(Page page, List<L> lines)
        {
            for (int li = 0; li < lines.Count; li++)
            {
                var line = lines[li];
                var tokens = line.NormTokens;

                bool hasLine = false, hasDesc = false;
                for (int t = 0; t < tokens.Length; t++)
                {
                    if (tokens[t] == "LINE") hasLine = true;
                    if (tokens[t] == "DESCRIPTION" || tokens[t] == "ITEM") hasDesc = true;
                }
                if (!hasLine || !hasDesc) continue;

                var colX = new List<(Col col, double x)>();

                void tryAdd(Col c, string[] aliases)
                {
                    for (int i = 0; i < line.Words.Count; i++)
                    {
                        var w = line.Words[i];
                        for (int a = 0; a < aliases.Length; a++)
                        {
                            if (w.Norm == NormToken(aliases[a])) { colX.Add((c, w.Box.Left)); return; }
                        }
                    }
                }

                tryAdd(Col.Line, HeaderAliases[(int)Col.Line]);
                tryAdd(Col.Qty, HeaderAliases[(int)Col.Qty]);
                tryAdd(Col.QtyStaged, HeaderAliases[(int)Col.QtyStaged]);
                tryAdd(Col.Description, HeaderAliases[(int)Col.Description]);
                tryAdd(Col.Width, HeaderAliases[(int)Col.Width]);
                tryAdd(Col.Length, HeaderAliases[(int)Col.Length]);
                tryAdd(Col.Weight, HeaderAliases[(int)Col.Weight]);
                tryAdd(Col.Unit, HeaderAliases[(int)Col.Unit]);

                if (!colX.Any(c => c.col == Col.Line) || !colX.Any(c => c.col == Col.Description)) continue;

                colX.Sort((a, b) => a.x.CompareTo(b.x));

                var spans = new Spans();
                for (int i = 0; i < colX.Count; i++)
                {
                    double left = (i == 0) ? 0 : (colX[i - 1].x + colX[i].x) / 2.0;
                    double right = (i == colX.Count - 1) ? page.Width : (colX[i].x + colX[i + 1].x) / 2.0;
                    spans.Map[colX[i].col] = (left, right);
                }
                return spans;
            }
            return null;
        }

        private static string ReadSpanText(L line, (double left, double right) span)
        {
            var seq = line.Words.Where(w => w.X >= span.left && w.X <= span.right).Select(w => w.Raw);
            return JoinRaw(seq);
        }

        private static string ExtractUnitToken(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                bool alpha = true;
                for (int i = 0; i < p.Length; i++) if (!char.IsLetter(p[i])) { alpha = false; break; }

                if (alpha && p.Length <= 6) return p.ToUpperInvariant(); // EA, PCS, LBS, etc.
            }
            return null;
        }

        private List<PickingListItem> ParseLineItems(PdfDocument doc)
        {
            var items = new List<PickingListItem>();

            foreach (var page in doc.GetPages())
            {
                var lines = GetLines(page);
                var spans = BuildSpansFromHeader(page, lines);
                if (spans == null) continue;

                int headIdx = FindTableHeaderIndex(lines);
                if (headIdx < 0) continue;

                for (int i = headIdx + 1; i < lines.Count; i++)
                {
                    var line = lines[i];

                    var lineCell = spans.Map.ContainsKey(Col.Line) ? ReadSpanText(line, spans.Map[Col.Line]) : null;
                    if (!int.TryParse(lineCell?.Trim(), out var lineNo) || lineNo <= 0) continue;

                    var qtyCell = spans.Map.ContainsKey(Col.Qty) ? ReadSpanText(line, spans.Map[Col.Qty]) : null;
                    var unitCell = spans.Map.ContainsKey(Col.Unit) ? ReadSpanText(line, spans.Map[Col.Unit]) : null;
                    var descCell = spans.Map.ContainsKey(Col.Description) ? ReadSpanText(line, spans.Map[Col.Description]) : null;
                    var widthCell = spans.Map.ContainsKey(Col.Width) ? ReadSpanText(line, spans.Map[Col.Width]) : null;
                    var lenCell = spans.Map.ContainsKey(Col.Length) ? ReadSpanText(line, spans.Map[Col.Length]) : null;
                    var wtCell = spans.Map.ContainsKey(Col.Weight) ? ReadSpanText(line, spans.Map[Col.Weight]) : null;

                    decimal qty = 0m;
                    if (!string.IsNullOrWhiteSpace(qtyCell))
                    {
                        var parts = qtyCell.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var p in parts)
                        {
                            var n = ParseDecimalLoose(p);
                            if (n.HasValue) { qty = n.Value; break; }
                        }
                    }
                    var unit = ExtractUnitToken(unitCell) ?? ExtractUnitToken(qtyCell) ?? "EA";

                    string longDesc = null;
                    if (i + 1 < lines.Count)
                    {
                        var next = lines[i + 1];
                        var nextLineCell = spans.Map.ContainsKey(Col.Line) ? ReadSpanText(next, spans.Map[Col.Line]) : null;
                        if (!int.TryParse(nextLineCell?.Trim(), out _))
                        {
                            var d = spans.Map.ContainsKey(Col.Description) ? ReadSpanText(next, spans.Map[Col.Description]) : null;
                            if (!string.IsNullOrWhiteSpace(d) && HasAnyLetter(d)) longDesc = d;
                        }
                    }

                    items.Add(new PickingListItem
                    {
                        LineNumber = lineNo,
                        Quantity = qty,
                        Unit = unit,
                        ItemId = ExtractItemId(descCell) ?? "UNKNOWN",
                        ItemDescription = (longDesc ?? (descCell ?? string.Empty)).Trim(),
                        Width = widthCell != null ? ParseDecimalLoose(widthCell) : null,
                        Length = lenCell != null ? ParseDecimalLoose(lenCell) : null,
                        Weight = wtCell != null ? ParseDecimalLoose(wtCell) : null
                    });
                }
            }

            return items;
        }

        private static string ExtractItemId(string descCell)
        {
            if (string.IsNullOrWhiteSpace(descCell)) return null;
            var m = Regex.Match(descCell, @"\b([A-Z0-9]+[A-Z0-9/]*[A-Z0-9])\b");
            if (m.Success && m.Value.Any(char.IsDigit)) return m.Value;
            return null;
        }
    }
}
