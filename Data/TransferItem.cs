using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    // Represents an item for branch stock transfer, simplified for now.
    public class TransferItem
    {
        public int Id { get; set; }
        [Required, MaxLength(64)]
        public string? SKU { get; set; }
        [Required, MaxLength(256)]
        public string? Description { get; set; }
        [Precision(18, 3)]
        public decimal Weight { get; set; }
    }
}
