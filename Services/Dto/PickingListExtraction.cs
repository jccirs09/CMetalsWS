using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CMetalsWS.Services.Dto
{
    public class PickingListExtraction
    {
        public string SalesOrderNumber { get; set; } = string.Empty;
        public string? OrderDate { get; set; }
        public string? ShipDate { get; set; }
        public string SoldTo { get; set; } = string.Empty;
        public string ShipTo { get; set; } = string.Empty;
        public string? SalesRep { get; set; }
        public string? ShippingVia { get; set; }
        public string? FOB { get; set; }
        public List<PickingListExtractionItem> Items { get; set; } = new();
        public decimal TotalWeightComputed { get; set; }
        public decimal? TotalWeightListed { get; set; }
        public decimal TotalWeightDelta { get; set; }
        public bool TotalWeightMatch { get; set; }
    }

    public class PickingListExtractionItem
    {
        public int? LineNumber { get; set; }
        public decimal? Quantity { get; set; }
        public string? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public decimal? Weight { get; set; }
        public string? Uom { get; set; }
    }
}
