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

            var pl = new PickingList
            {
                BranchId = branchId,
                CustomerId = customerId,
                TruckId = truckId
            };

            // Use single-line regex matching on the entire text block
            pl.SalesOrderNumber = MatchAndGetGroup(text, @"PICKING LISTNo\.(?<val>[\d\w]+)");
            pl.ShipDate = ParseDate(MatchAndGetGroup(text, @"SHIP DATE(?<val>\d{2}/\d{2}/\d{4})"));
            pl.OrderDate = ParseDate(MatchAndGetGroup(text, @"ORDER DATE(?<val>\d{2}/\d{2}/\d{4})"));
            pl.ShippingMethod = MatchAndGetGroup(text, @"SHIP VIA(?<val>[\w\s]+?)SOLD TO");

            var soldToMatch = Regex.Match(text, @"SOLD TO(?<sold>.*?)SHIP TO");
            if(soldToMatch.Success)
            {
                pl.CustomerName = soldToMatch.Groups["sold"].Value.Trim();
            }

            var shipToMatch = Regex.Match(text, @"SHIP TO(?<ship>.*?)SHIP VIA");
            if(shipToMatch.Success)
            {
                pl.ShipToAddress = shipToMatch.Groups["ship"].Value.Trim();
            }

            // Item parsing
            var itemsTextMatch = Regex.Match(text, @"LINE QUANTITY QTY STAGED DESCRIPTION WIDTH LENGTH WEIGHT(?<items>.*)PULLED BY");
            if (itemsTextMatch.Success)
            {
                var itemsText = itemsTextMatch.Groups["items"].Value;
                var itemMatches = Regex.Matches(itemsText, @"(?<line>\d+)\s+(?<qty>\d+)\s+PCS\s+(?<itemid>[\w\d]+)\s+(?<desc>.*?)(?<width>\d+"")\s+(?<length>\d+\.?\d*"")\s+(?<weight>[\d,\.]+)");

                foreach (Match itemMatch in itemMatches)
                {
                    var item = new PickingListItem
                    {
                        LineNumber = ToInt(itemMatch.Groups["line"].Value),
                        Quantity = ToDec(itemMatch.Groups["qty"].Value),
                        Unit = "PCS",
                        ItemId = itemMatch.Groups["itemid"].Value,
                        ItemDescription = itemMatch.Groups["desc"].Value.Trim(),
                        Width = ToDec(itemMatch.Groups["width"].Value),
                        Length = ToDec(itemMatch.Groups["length"].Value),
                        Weight = ToDec(itemMatch.Groups["weight"].Value)
                    };
                    pl.Items.Add(item);
                }
            }

            return pl;
        }

        private static string MatchAndGetGroup(string text, string pattern, string groupName = "val")
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[groupName].Value.Trim() : string.Empty;
        }

        private static string ExtractText(Stream pdfStream)
        {
            if (pdfStream.CanSeek) pdfStream.Position = 0;
            var text = new StringBuilder();
            using (var pdf = PdfDocument.Open(pdfStream))
            {
                foreach (var page in pdf.GetPages())
                {
                    text.Append(" " + page.Text);
                }
            }
            return text.ToString();
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
            if (int.TryParse(s?.Replace(",", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) return v;
            return null;
        }

        private static decimal? ToDec(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var cleaned = s.Replace(",", "").Replace("\"", "");
            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            return null;
        }
    }
}
