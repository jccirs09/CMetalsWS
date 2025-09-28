using MiniExcelLibs.Attributes;

namespace CMetalsWS.Models
{
    public class CustomerImportDto
    {
        [ExcelColumnName("CustomerCode")]
        public string CustomerCode { get; set; } = string.Empty;

        [ExcelColumnName("CustomerName")]
        public string CustomerName { get; set; } = string.Empty;

        [ExcelColumnName("FullAddress")]
        public string? FullAddress { get; set; }

        [ExcelColumnName("BusinessHours")]
        public string? BusinessHours { get; set; }

        [ExcelColumnName("ContactNumber")]
        public string? ContactNumber { get; set; }

        [ExcelColumnName("Latitude")]
        public double? Latitude { get; set; }

        [ExcelColumnName("Longitude")]
        public double? Longitude { get; set; }
    }
}