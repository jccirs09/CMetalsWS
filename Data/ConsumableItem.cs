using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Data
{
    [Index(nameof(ItemCode), IsUnique = true)]
    [Index(nameof(IsActive))]
    public class ConsumableItem
    {
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string ItemCode { get; set; } = string.Empty;

        [Required, MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        [Required, MaxLength(16)]
        public string UnitOfMeasure { get; set; } = string.Empty;

        public int MinQuantity { get; set; }

        public int MaxQuantity { get; set; }

        public int CurrentQuantity { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
