using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class OrderItemFulfillment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PickingListItemId { get; set; }

        [ForeignKey(nameof(PickingListItemId))]
        public virtual PickingListItem PickingListItem { get; set; } = default!;

        [Required]
        public FulfillmentType FulfillmentType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal FulfilledQuantity { get; set; }

        [Required]
        public DateTime FulfillmentDate { get; set; }

        [Required]
        public string RecordedById { get; set; } = default!;

        [ForeignKey(nameof(RecordedById))]
        public virtual ApplicationUser RecordedBy { get; set; } = default!;

        public int? LoadId { get; set; }

        [ForeignKey(nameof(LoadId))]
        public virtual Load? Load { get; set; }

        [MaxLength(512)]
        public string? Notes { get; set; }
    }
}