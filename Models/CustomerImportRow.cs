using CMetalsWS.Data;
using System.Collections.Generic;

namespace CMetalsWS.Models
{
    public class CustomerImportRow
    {
        public CustomerImportDto Dto { get; set; } = new();
        public string? Error { get; set; }
    }
}
