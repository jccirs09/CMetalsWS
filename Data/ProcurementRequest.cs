using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class ProcurementRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(32)]
        public string RequestNumber { get; set; } = string.Empty;

        public int ConsumableItemId { get; set; }
        [ForeignKey(nameof(ConsumableItemId))]
        public virtual ConsumableItem? ConsumableItem { get; set; }

        public int QuantityRequested { get; set; }

        [Required, MaxLength(256)]
        public string RequestedBy { get; set; } = string.Empty;

        public DateTime RequestedDate { get; set; }

        [MaxLength(128)]
        public string? SupplierName { get; set; }

        public ProcurementRequestStatus Status { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        [MaxLength(512)]
        public string? Notes { get; set; }
    }
}
