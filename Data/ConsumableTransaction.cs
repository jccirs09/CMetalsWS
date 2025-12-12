using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class ConsumableTransaction
    {
        public int Id { get; set; }

        public int ConsumableItemId { get; set; }
        [ForeignKey(nameof(ConsumableItemId))]
        public virtual ConsumableItem? ConsumableItem { get; set; }

        public ConsumableTransactionType TransactionType { get; set; }

        public int Quantity { get; set; }

        public DateTime Timestamp { get; set; }

        [Required, MaxLength(256)]
        public string PerformedBy { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? Reference { get; set; }

        [MaxLength(512)]
        public string? Notes { get; set; }
    }
}
