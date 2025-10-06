using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class DestinationRegion
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string Name { get; set; } = default!;
        [MaxLength(64)]
        public string Type { get; set; } = "local";
        [MaxLength(256)]
        public string? Description { get; set; }
        public bool RequiresPooling { get; set; }
        public bool IsCustomerPickup { get; set; }
        public string? CoordinatorId { get; set; }
        [ForeignKey(nameof(CoordinatorId))]
        public virtual ApplicationUser? Coordinator { get; set; }
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        [MaxLength(64)]
        public string Icon { get; set; } = string.Empty;
        [MaxLength(32)]
        public string Color { get; set; } = "grey";
    }
}
