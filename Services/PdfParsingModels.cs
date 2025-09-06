using System.Text.Json.Serialization;

namespace CMetalsWS.Services
{
    public class PickingListItemDto
    {
        [JsonPropertyName("LineNumber")]
        public int LineNumber { get; set; }

        [JsonPropertyName("Quantity")]
        public decimal Quantity { get; set; }

        [JsonPropertyName("ItemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("ItemDescription")]
        public string ItemDescription { get; set; } = string.Empty;

        [JsonPropertyName("Width")]
        public object? Width { get; set; }

        [JsonPropertyName("Length")]
        public object? Length { get; set; }

        [JsonPropertyName("Weight")]
        public decimal? Weight { get; set; }

        [JsonPropertyName("Unit")]
        public string Unit { get; set; } = "EA";
    }
}
