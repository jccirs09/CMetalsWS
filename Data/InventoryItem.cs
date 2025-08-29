using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Data
{
    [Index(nameof(BranchId))]
    [Index(nameof(ItemId))]
    public class InventoryItem
    {
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required, MaxLength(64)]
        public string ItemId { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        [MaxLength(64)]
        public string? TagNumber { get; set; }

        [Precision(18, 3)]
        public decimal? Width { get; set; }

        [Precision(18, 3)]
        public decimal? Length { get; set; }

        // Replaces Weight: a flexible snapshot that can represent PCS (sheets) or LBS (coils)
        [Precision(18, 3)]
        public decimal? Snapshot { get; set; }

        [MaxLength(8)]
        public string? SnapshotUnit { get; set; } // PCS or LBS

        [MaxLength(64)]
        public string? Location { get; set; }

        // optional extras you already had in the import sheet
        [MaxLength(64)]
        public string? Status { get; set; }

        [MaxLength(32)]
        public string? SnapshotLabel { get; set; } // optional display label if you want one later
    }


}