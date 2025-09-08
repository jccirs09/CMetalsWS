using CMetalsWS.Data;
using System.Collections.Generic;

namespace CMetalsWS.Models
{
    public class CustomerImportRow
    {
        public CustomerImportDto Dto { get; set; } = new();
        public List<Customer> Candidates { get; set; } = new();
        public string? SelectedPlaceId { get; set; }
        public bool RequiresManualSelection => Candidates.Any() && string.IsNullOrWhiteSpace(SelectedPlaceId);
        public string? Error { get; set; }
    }
}
