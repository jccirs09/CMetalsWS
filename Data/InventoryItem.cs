using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    public enum InventoryStatus { Available = 0, Reserved = 1, Allocated = 2, Shipped = 3, Lost = 4 }

    [Index(nameof(ItemId), nameof(TagNumber), nameof(BranchId), IsUnique = true)]
    public class InventoryItem
    {
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string ItemId { get; set; } = default!;

        [Required, MaxLength(256)]
        public string Description { get; set; } = default!;

        [Required, MaxLength(64)]
        public string TagNumber { get; set; } = default!;

        [Precision(18, 3)]
        public decimal? Width { get; set; }

        [Precision(18, 3)]
        public decimal? Length { get; set; }

        [Precision(18, 3)]
        public decimal? Weight { get; set; }

        [MaxLength(64)]
        public string? Location { get; set; }

        public InventoryStatus Status { get; set; } = InventoryStatus.Available;

        [Required]
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}
