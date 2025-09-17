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

        public PdfParsingService(ILogger<PdfParsingService> logger)
        {
            _logger = logger;
        }

        public Task<(PickingList, List<PickingListItem>)> ParsePickingListAsync(byte[] pdfBytes)
        {
            using var doc = PdfDocument.Open(pdfBytes);

            var header = ParseHeader(doc);
            var items  = ParseLineItems(doc);

            if (header.TotalWeight == 0 && items.Any(i => i.Weight.HasValue))
                header.TotalWeight = items.Where(i => i.Weight.HasValue).Sum(i => i.Weight!.Value);

            return Task.FromResult((header, items));
        }

        // ---------- small helpers ----------
        private static string NormToken(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = s.Trim();
            if (s.EndsWith(":", StringComparison.Ordinal)) s = s[..^1];
            return s.ToUpperInvariant();
        }

        private static bool HasAnyLetter(string s)
        {
            foreach (var ch in s) if (char.IsLetter(ch)) return true;
            return false;
        }

        private static decimal? ParseDecimalLoose(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                if ((c >= '0' && c <= '9') || c == '.' || c == '-')
                    sb.Append(c);
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

        private static string JoinRaw(IEnumerable<string> tokens)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var t in tokens)
            {
                if (string.IsNullOrEmpty(t)) continue;
                if (!first) sb.Append(' ');
                sb.Append(t);
                first = false;
            }
            return sb.ToString();
        }

        // -------------------- page → lines model --------------------
        public sealed class W
        {
            public Word Word;
            public string Raw;
            public string Norm;
            public double X;
            public double Y;
            public PdfRectangle Box;
        }

        public sealed class L
        {
            public List<W> Words;  // sorted by X asc
            public double Y;
            private string[] _normTokensCache;

            public L(List<W> words, double y)
            {
                Words = words;
                Y = y;
            }

            public string RawLineText { get { return JoinRaw(Words.Select(w => w.Raw)); } }

            public string[] NormTokens
            {
                get
                {
                    if (_normTokensCache == null)
                        _normTokensCache = Words.Select(w => w.Norm).ToArray();
                    return _normTokensCache;
                }
            }
        }

        /// Lines are sorted top→bottom (PDF Y desc). Larger index == lower on page.
        public static List<L> GetLines(Page page, double yTol = 1.6)
        {
            var words = page.GetWords().ToList(); // materialize for Count/2nd pass

            var list = new List<W>(Math.Max(16, words.Count));
            foreach (var w in words)
            {
                var box = w.BoundingBox;
                var cent = box.Centroid;
                list.Add(new W
                {
                    Word = w,
                    Raw  = w.Text,
                    Norm = NormToken(w.Text),
                    X    = cent.X,
                    Y    = cent.Y,
                    Box  = box
                });
            }

            var lines = list
                .GroupBy(w => Math.Round(w.Y / yTol) * yTol)
                .OrderByDescending(g => g.Key) // top first
                .Select(g => new L(g.OrderBy(x => x.X).ToList(), g.Key))
                .ToList();

            return lines;
        }

        // -------------------- label finders & readers --------------------
        private static (int lineIndex, PdfRectangle box)? FindLabelBox(List<L> lines, params string[] aliases)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var tokens = line.NormTokens;

                for (int a = 0; a < aliases.Length; a++)
                {
                    var parts = aliases[a]
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => NormToken(s))
                        .ToArray();
                    if (parts.Length == 0) continue;

                    for (int start = 0; start <= tokens.Length - parts.Length; start++)
                    {
                        bool match = true;
                        for (int k = 0; k < parts.Length; k++)
                        {
                            if (!tokens[start + k].Equals(parts[k], StringComparison.Ordinal))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match) continue;

                        // union box
                        var first = line.Words[start].Box;
                        var box = first;
                        for (int k = 1; k < parts.Length; k++)
                        {
                            var next = line.Words[start + k].Box;
                            box = new PdfRectangle(
                                Math.Min(box.Left, next.Left),
                                Math.Min(box.Bottom, next.Bottom),
                                Math.Max(box.Right, next.Right),
                                Math.Max(box.Top, next.Top));
                        }
                        return (i, box);
                    }
                }
            }
            return null;
        }

        private static string SameLineRightText(L line, PdfRectangle labelBox, double gapX, double maxX)
        {
            var seq = line.Words.Where(w => w.Box.Left > labelBox.Right + gapX &&
                                            w.Box.Left < labelBox.Right + maxX)
                                .Select(w => w.Raw);
            return JoinRaw(seq);
        }

        /// simple span reader for a single line
        private static string ReadSpan(L line, double left, double right)
        {
            var seq = line.Words.Where(w => w.X >= left && w.X <= right).Select(w => w.Raw);
            return JoinRaw(seq);
        }

        private static readonly HashSet<string> StopTokens = new HashSet<string>(StringComparer.Ordinal)
        {
            // table headers
            "LINE","QUANTITY","QTY","QTY STAGED","DESCRIPTION","ITEM","WIDTH","LENGTH","WEIGHT","UNIT","UOM",
            // boilerplate / notes
            "RECEIVING","HOURS","DELIVERED","TERMS","MAX","SKID","ROUTE","MILL","CERTS","SOURCE","TAG","HEAT","PULLED","BY","TOTAL","WT",
            // other header labels
            "SOLD","REP","ORDER","DATE","FOB","PRINT","PRINTED","BUYER","PURCHASE"
        };

        /// <summary>
        /// Read up to <paramref name="maxLines"/> lines *below* the label (lower on page).
        /// </summary>
        private static string BlockBelow(List<L> lines, int labelLineIndex, PdfRectangle labelBox,
                                         int maxLines, double leftPad, double rightPad)
        {
            if (labelLineIndex < 0 || labelLineIndex >= lines.Count) return null;

            double left = labelBox.Left + leftPad;
            double right = labelBox.Left + rightPad;

            var kept = new List<string>(maxLines);

            // below == larger index.
            for (int i = labelLineIndex + 1; i < lines.Count && kept.Count < maxLines; i++)
            {
                var line = lines[i];
                var text = ReadSpan(line, left, right).Trim();
                if (text.Length == 0) break; // Stop on empty line
                kept.Add(text);
            }

            if (kept.Count == 0) return null;
            return JoinRaw(kept);
        }

        /// Read a 3-column grid row (labels) and take values from the very next line
        private static bool TryReadGridRow3(Page page, List<L> lines,
                                            string[] c1Labels, string[] c2Labels, string[] c3Labels,
                                            out string v1, out string v2, out string v3)
        {
            v1 = v2 = v3 = null;

            var b1 = FindLabelBox(lines, c1Labels);
            var b2 = FindLabelBox(lines, c2Labels);
            var b3 = FindLabelBox(lines, c3Labels);
            if (b1 == null || b2 == null || b3 == null) return false;

            var li = b1.Value.lineIndex;
            if (b2.Value.lineIndex != li || b3.Value.lineIndex != li) return false;
            if (li + 1 >= lines.Count) return false;

            var boxes = new List<(int idx, PdfRectangle box)>
            {
                (0, b1.Value.box), (1, b2.Value.box), (2, b3.Value.box)
            };
            boxes.Sort((a, b) => a.box.Left.CompareTo(b.box.Left));

            double left0  = 0;
            double mid01  = (boxes[0].box.Right + boxes[1].box.Left) / 2.0;
            double mid12  = (boxes[1].box.Right + boxes[2].box.Left) / 2.0;
            double right2 = page.Width;

            var next = lines[li + 1];
            var s0 = ReadSpan(next, left0, mid01);
            var s1 = ReadSpan(next, mid01, mid12);
            var s2 = ReadSpan(next, mid12, right2);

            var arr = new[] { s0, s1, s2 };
            // map back to original order
            var map = new string[3];
            for (int i = 0; i < 3; i++)
            {
                int pos = boxes.FindIndex(b => b.idx == i);
                map[i] = arr[pos];
            }
            v1 = map[0];
            v2 = map[1];
            v3 = map[2];
            return true;
        }

        private static string? ReadValueForLabel(List<L> lines, params string[] aliases)
        {
            var label = FindLabelBox(lines, aliases);
            if (label == null) return null;

            var line = lines[label.Value.lineIndex];
            var text = SameLineRightText(line, label.Value.box, 0.5, 300);
            if (!string.IsNullOrWhiteSpace(text)) return text;

            return BlockBelow(lines, label.Value.lineIndex, label.Value.box, 1, -2, 300);
        }

        // -------------------- header parsing --------------------
        private PickingList ParseHeader(PdfDocument doc)
        {
            var page  = doc.GetPage(1);
            var lines = GetLines(page);

            // Simple labels (addresses)
            var soldToLbl    = FindLabelBox(lines, "SOLD TO");
            var shipToLbl    = FindLabelBox(lines, "SHIP TO");
            var printLbl     = FindLabelBox(lines, "PRINT DATE/TIME", "PRINT DATE", "PRINTED");
            var totWtLbl     = FindLabelBox(lines, "TOTAL WEIGHT", "TOTAL WT");

            var soldTo = soldToLbl != null ? BlockBelow(lines, soldToLbl.Value.lineIndex, soldToLbl.Value.box, 3, -2, 340) : null;
            var shipTo = shipToLbl != null ? BlockBelow(lines, shipToLbl.Value.lineIndex, shipToLbl.Value.box, 3, -2, 340) : null;

            // Grid rows:
            //   [JOB NAME | SALES REP | SHIP VIA] -> read next line (SalesRep, ShipVia)
            string jobName = null, salesRep = null, shipVia = null;
            TryReadGridRow3(page, lines,
                new[] { "JOB NAME" }, new[] { "SALES REP", "SALESPERSON" }, new[] { "SHIP VIA", "SHIPPING VIA" },
                out jobName, out salesRep, out shipVia);

            if (string.IsNullOrWhiteSpace(shipVia))
            {
                shipVia = ReadValueForLabel(lines, "SHIP VIA", "SHIPPING VIA");
            }

            //   [PICKING GROUP | BUYER | SHIP DATE] -> next line (ShipDate)
            string grp = null, buyerGrid = null, shipDateGrid = null;
            TryReadGridRow3(page, lines,
                new[] { "PICKING GROUP" }, new[] { "BUYER" }, new[] { "SHIP DATE" },
                out grp, out buyerGrid, out shipDateGrid);

            var shipDateText = shipDateGrid ?? ReadValueForLabel(lines, "SHIP DATE");

            // ORDER DATE: label appears on a different line; its value is on the next line (same column)
            var orderDateText = ReadValueForLabel(lines, "ORDER DATE");

            // FOB on same line or below
            var fob = ReadValueForLabel(lines, "FOB", "FOB POINT", "F.O.B.");

            // Print date/time
            var print = printLbl != null
                ? (SameLineRightText(lines[printLbl.Value.lineIndex], printLbl.Value.box, 0.5, 300)
                   ?? BlockBelow(lines, printLbl.Value.lineIndex, printLbl.Value.box, 1, -2, 300))
                : null;

            // Total weight
            decimal totalWeight = 0m;
            if (totWtLbl != null)
            {
                var s = SameLineRightText(lines[totWtLbl.Value.lineIndex], totWtLbl.Value.box, 0.5, 200)
                        ?? BlockBelow(lines, totWtLbl.Value.lineIndex, totWtLbl.Value.box, 1, -2, 200);
                if (!string.IsNullOrWhiteSpace(s))
                    totalWeight = ParseDecimalLoose(s) ?? 0m;
            }

            var list = new PickingList
            {
                SalesOrderNumber = ReadValueForLabel(lines, "PICKING LIST NO", "PICKING LIST #", "PICKING LIST NUMBER", "PICK LIST NO", "PICK LIST #", "P/L NO", "PL NO", "PICKING LIST") ?? string.Empty,
                OrderDate        = ParseDateLoose(orderDateText ?? string.Empty),
                ShipDate         = ParseDateLoose(shipDateText ?? string.Empty),
                SoldTo           = soldTo,
                ShipTo           = shipTo,
                SalesRep         = string.IsNullOrWhiteSpace(salesRep) ? null : salesRep.Trim(),
                ShippingVia      = string.IsNullOrWhiteSpace(shipVia) ? null : shipVia.Trim(),
                FOB              = fob,
                Buyer            = string.IsNullOrWhiteSpace(buyerGrid) ? null : buyerGrid.Trim(),
                PrintDateTime    = ParseDateLoose(print ?? string.Empty),
                TotalWeight      = totalWeight
            };

            if (string.IsNullOrWhiteSpace(list.SalesOrderNumber))
            {
                // top-band fallback (first ~6 lines)
                var top = string.Join(" ", lines.Take(6).Select(l => l.RawLineText));
                var m = Regex.Match(top, @"\b\d{6,}\b");
                if (m.Success) list.SalesOrderNumber = m.Value;
            }

            return list;
        }

        // -------------------- table parsing --------------------
        private enum Col { Line, Qty, QtyStaged, Description, Width, Length, Weight, Unit }

        private static readonly string[][] HeaderAliases =
        {
            new [] { "LINE" },
            new [] { "QUANTITY", "QTY" },
            new [] { "QTY STAGED", "QTY_STAGED", "STAGED" }, // helper only
            new [] { "DESCRIPTION", "ITEM", "ITEM DESCRIPTION" },
            new [] { "WIDTH" },
            new [] { "LENGTH" },
            new [] { "WEIGHT", "WT", "LBS" },
            new [] { "UNIT", "UOM" }
        };

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

                Action<Col, string[]> tryAdd = (c, aliases) =>
                {
                    for (int i = 0; i < line.Words.Count; i++)
                    {
                        var w = line.Words[i];
                        for (int a = 0; a < aliases.Length; a++)
                        {
                            if (w.Norm == NormToken(aliases[a]))
                            {
                                colX.Add((c, w.Box.Left));
                                return;
                            }
                        }
                    }
                };

                tryAdd(Col.Line,        HeaderAliases[(int)Col.Line]);
                tryAdd(Col.Qty,         HeaderAliases[(int)Col.Qty]);
                tryAdd(Col.QtyStaged,   HeaderAliases[(int)Col.QtyStaged]);
                tryAdd(Col.Description, HeaderAliases[(int)Col.Description]);
                tryAdd(Col.Width,       HeaderAliases[(int)Col.Width]);
                tryAdd(Col.Length,      HeaderAliases[(int)Col.Length]);
                tryAdd(Col.Weight,      HeaderAliases[(int)Col.Weight]);
                tryAdd(Col.Unit,        HeaderAliases[(int)Col.Unit]);

                if (!colX.Any(c => c.col == Col.Line) || !colX.Any(c => c.col == Col.Description))
                    continue;

                colX.Sort((a, b) => a.x.CompareTo(b.x));

                var spans = new Spans();
                for (int i = 0; i < colX.Count; i++)
                {
                    double left  = (i == 0) ? 0 : (colX[i - 1].x + colX[i].x) / 2.0;
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
                for (int i = 0; i < p.Length; i++)
                    if (!char.IsLetter(p[i])) { alpha = false; break; }

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

                // find header line index
                int headIdx = -1;
                for (int i = 0; i < lines.Count; i++)
                {
                    var t = lines[i].NormTokens;
                    bool hasLine = false, hasDesc = false;
                    for (int k = 0; k < t.Length; k++)
                    {
                        if (t[k] == "LINE") hasLine = true;
                        if (t[k] == "DESCRIPTION" || t[k] == "ITEM") hasDesc = true;
                    }
                    if (hasLine && hasDesc) { headIdx = i; break; }
                }
                if (headIdx < 0) continue;

                // iterate rows below header (larger index)
                for (int i = headIdx + 1; i < lines.Count; i++)
                {
                    var line = lines[i];

                    var lineCell = spans.Map.ContainsKey(Col.Line) ? ReadSpanText(line, spans.Map[Col.Line]) : null;
                    int lineNo;
                    if (!int.TryParse(lineCell != null ? lineCell.Trim() : null, out lineNo) || lineNo <= 0)
                        continue;

                    var qtyCell   = spans.Map.ContainsKey(Col.Qty)         ? ReadSpanText(line, spans.Map[Col.Qty])         : null;
                    var unitCell  = spans.Map.ContainsKey(Col.Unit)        ? ReadSpanText(line, spans.Map[Col.Unit])        : null;
                    var descCell  = spans.Map.ContainsKey(Col.Description) ? ReadSpanText(line, spans.Map[Col.Description]) : null;
                    var widthCell = spans.Map.ContainsKey(Col.Width)       ? ReadSpanText(line, spans.Map[Col.Width])       : null;
                    var lenCell   = spans.Map.ContainsKey(Col.Length)      ? ReadSpanText(line, spans.Map[Col.Length])      : null;
                    var wtCell    = spans.Map.ContainsKey(Col.Weight)      ? ReadSpanText(line, spans.Map[Col.Weight])      : null;

                    // quantity + unit
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

                    // pick human description from the next non-line-number row (usually the very next row)
                    string longDesc = null;
                    if (i + 1 < lines.Count)
                    {
                        var next = lines[i + 1];
                        string nextLineCell = spans.Map.ContainsKey(Col.Line) ? ReadSpanText(next, spans.Map[Col.Line]) : null;
                        int dummy;
                        if (!int.TryParse(nextLineCell != null ? nextLineCell.Trim() : null, out dummy))
                        {
                            string d = spans.Map.ContainsKey(Col.Description) ? ReadSpanText(next, spans.Map[Col.Description]) : null;
                            if (!string.IsNullOrWhiteSpace(d) && HasAnyLetter(d)) longDesc = d;
                        }
                    }

                    items.Add(new PickingListItem
                    {
                        LineNumber      = lineNo,
                        Quantity        = qty,
                        Unit            = unit,
                        ItemId          = ExtractItemId(descCell) ?? "UNKNOWN",
                        ItemDescription = (longDesc ?? (descCell ?? string.Empty)).Trim(),
                        Width           = widthCell != null ? ParseDecimalLoose(widthCell) : null,
                        Length          = lenCell   != null ? ParseDecimalLoose(lenCell)   : null,
                        Weight          = wtCell    != null ? ParseDecimalLoose(wtCell)    : null
                    });
                }
            }

            return items;
        }

        private static string ExtractItemId(string descCell)
        {
            if (string.IsNullOrWhiteSpace(descCell)) return null;
            // capture token containing digits (PPS2435, 3PVC, PP2448BWB, PP2448IO/C)
            var m = Regex.Match(descCell, @"\b([A-Z0-9]+[A-Z0-9/]*[A-Z0-9])\b");
            if (m.Success && m.Value.Any(char.IsDigit)) return m.Value;
            return null;
        }
    }
}
