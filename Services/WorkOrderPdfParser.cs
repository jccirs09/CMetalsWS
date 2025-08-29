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
    public class WorkOrderPdfParser : IWorkOrderPdfParser
    {
        public WorkOrder Parse(Stream pdfStream, int branchId)
        {
            var text = ExtractText(pdfStream);
            var lines = SplitLines(text);

            var wo = new WorkOrder
            {
                BranchId = branchId,
                MachineCategory = MachineCategory.CTL
            };

            // --- Header ---
            wo.PdfWorkOrderNumber = MatchFirst(lines, @"CTL\s+ORDER\s+#(?<num>\d+)", "num");
            var dueDateStr = MatchFirst(lines, @"DATE\s+DUE:\s+(?<dt>\d{2}/\d{2}/\d{4})", "dt");
            if (DateTime.TryParseExact(dueDateStr, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate))
            {
                wo.DueDate = dueDate;
            }

            // --- Pull Info (Parent Coil) ---
            var pullInfoIdx = IndexOf(lines, l => l.Contains("PULL INFO"));
            var skidSetupsIdx = IndexOf(lines, l => l.Contains("SKID SETUPS"));
            if (pullInfoIdx >= 0 && skidSetupsIdx > pullInfoIdx)
            {
                var pullInfoLines = lines.Skip(pullInfoIdx + 1).Take(skidSetupsIdx - (pullInfoIdx + 1)).ToList();
                wo.TagNumber = MatchFirst(pullInfoLines, @"^(?<tag>\d+)", "tag") ?? string.Empty;
            }

            // --- Skid Setups (Child Items) ---
            if (skidSetupsIdx >= 0)
            {
                var itemLines = lines.Skip(skidSetupsIdx + 1).ToList();
                var headerIdx = IndexOf(itemLines, l => l.Contains("Skid") && l.Contains("Action"));
                if (headerIdx >= 0)
                {
                    // Regex for format 1 (with Parent Tag in the line)
                    var regex1 = new Regex(@"^\s*\d+\s+(?<action>\w+)\s+(?<childTag>\d+)\s+(?<parentTag>\d+)\s+(?<width>[\d\.]+)\s+(?<length>[\d\.]+)\s+(?<pcs>[\d,]+).*?(?<weight>[\d,]+)\s+.*?(?<itemId>ALUM\S+|PPS\S+)");

                    // Regex for format 2 (without Parent Tag in the line)
                    var regex2 = new Regex(@"^\s*\d+\s+(?<action>\w+)\s+(?<childTag>\d+)\s+(?<width>[\d\.]+)\s+(?<length>[\d\.]+)\s+(?<pcs>[\d,]+).*?(?<weight>[\d,]+)\s+.*?(?<itemId>ALUM\S+|PPS\S+)");

                    for (int i = headerIdx + 1; i < itemLines.Count; i++)
                    {
                        var line = itemLines[i];
                        if (line.Contains("SHIP-TO INFO") || line.Contains("END ORDER")) break;

                        var match = regex1.Match(line);
                        if (!match.Success)
                        {
                            match = regex2.Match(line);
                        }

                        if (match.Success)
                        {
                            var item = new WorkOrderItem
                            {
                                ItemCode = match.Groups["itemId"].Value,
                                OrderQuantity = ToDec(match.Groups["pcs"].Value),
                                Weight = ToDec(match.Groups["weight"].Value),
                                Width = ToDec(match.Groups["width"].Value),
                                Length = ToDec(match.Groups["length"].Value)
                            };
                            wo.Items.Add(item);
                        }
                    }
                }
            }

            return wo;
        }

        private static string ExtractText(Stream pdfStream)
        {
            var ocr = new IronTesseract();
            var input = new OcrInput();
            input.LoadPdf(pdfStream);
            var result = ocr.Read(input);
            return result.Text;
        }

        private static List<string> SplitLines(string text) => text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
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
        private static decimal? ToDec(string? s) => decimal.TryParse(s?.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }
}