namespace CMetalsWS.Models
{
    public class GooglePlacesTextSearchRequest
    {
        public string textQuery { get; set; } = string.Empty;
        public string? regionCode { get; set; }
        public LocationBias? locationBias { get; set; }
        public int maxResultCount { get; set; }
    }

    public class LocationBias
    {
        public Rectangle rectangle { get; set; } = new();
    }

    public class Rectangle
    {
        public LatLng low { get; set; } = new();
        public LatLng high { get; set; } = new();
    }

    public class LatLng
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
