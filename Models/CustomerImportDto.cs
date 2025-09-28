namespace CMetalsWS.Models
{
    public class CustomerImportDto
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? FullAddress { get; set; }
        public string? BusinessHours { get; set; }
        public string? ContactNumber { get; set; }
    }
}