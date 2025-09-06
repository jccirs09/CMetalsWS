using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace CMetalsWS.Services
{
    public class ParsingResultDto
    {
        [JsonPropertyName("header")]
        public PickingListHeaderDto? Header { get; set; }

        [JsonPropertyName("lineItems")]
        public List<PickingListItemDto> LineItems { get; set; } = new();
    }

    public class PickingListHeaderDto
    {
        [JsonPropertyName("salesOrderNumber")]
        public string SalesOrderNumber { get; set; } = string.Empty;

        [JsonPropertyName("orderDate")]
        public DateTime? OrderDate { get; set; }

        [JsonPropertyName("shipDate")]
        public DateTime? ShipDate { get; set; }

        [JsonPropertyName("soldTo")]
        public string? SoldTo { get; set; }

        [JsonPropertyName("shipTo")]
        public string? ShipTo { get; set; }

        [JsonPropertyName("salesRep")]
        public string? SalesRep { get; set; }

        [JsonPropertyName("shippingVia")]
        public string? ShippingVia { get; set; }

        [JsonPropertyName("fob")]
        public string? FOB { get; set; }

        [JsonPropertyName("buyer")]
        public string? Buyer { get; set; }

        [JsonPropertyName("printDateTime")]
        public DateTime? PrintDateTime { get; set; }

        [JsonPropertyName("totalWeight")]
        public decimal TotalWeight { get; set; }
    }

    public class PickingListItemDto
    {
        [JsonPropertyName("lineNumber")]
        public int LineNumber { get; set; }

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("itemDescription")]
        public string ItemDescription { get; set; } = string.Empty;

        [JsonPropertyName("width")]
        public object? Width { get; set; }

        [JsonPropertyName("length")]
        public object? Length { get; set; }

        [JsonPropertyName("weight")]
        public decimal? Weight { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "EA";
    }
}
