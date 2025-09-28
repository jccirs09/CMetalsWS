namespace CMetalsWS.Models
{
    public class CustomerImportDto
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Street1 { get; set; } = string.Empty;
        public string Street2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string BusinessHours { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }
}
