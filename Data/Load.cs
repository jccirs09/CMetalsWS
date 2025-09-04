using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class Load
    {
        public int Id { get; set; }
        [Required, MaxLength(64)]
        public string LoadNumber { get; set; } = default!;
        public int? TruckId { get; set; }
        public virtual Truck? Truck { get; set; }
        public DateTime? ShippingDate { get; set; }
        [Precision(18, 3)]
        public decimal TotalWeight { get; set; }
        public LoadStatus Status { get; set; }
        public int OriginBranchId { get; set; }
        public int? DestinationBranchId { get; set; }
        [MaxLength(512)]
        public string? Notes { get; set; }
        public virtual Branch OriginBranch { get; set; } = null!;
        public virtual Branch? DestinationBranch { get; set; }
        public virtual ICollection<LoadItem> Items { get; set; } = new List<LoadItem>();
    }

    public class LoadItem
    {
        public int Id { get; set; }
        public int LoadId { get; set; }
        public virtual Load Load { get; set; } = null!;
        public int PickingListId { get; set; }
        public virtual PickingList PickingList { get; set; } = null!;
        // We can also link to the specific line item for clarity
        public int PickingListItemId { get; set; }
        public virtual PickingListItem PickingListItem { get; set; } = null!;
        public int StopSequence { get; set; }
        [Precision(18, 3)]
        public decimal ShippedWeight { get; set; }
    }
}
