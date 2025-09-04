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
        public int TruckId { get; set; }
        public virtual Truck Truck { get; set; }
        public DateTime ShippingDate { get; set; }
        [Precision(18, 3)]
        public decimal TotalWeight { get; set; }
        public LoadStatus Status { get; set; }

        // --- ENHANCEMENTS ---
        public LoadType LoadType { get; set; }
        public int OriginBranchId { get; set; }
        public int? DestinationBranchId { get; set; }
        [MaxLength(512)]
        public string? Notes { get; set; }
        public virtual Branch OriginBranch { get; set; }
        public virtual Branch? DestinationBranch { get; set; }

        public virtual ICollection<LoadItem> Items { get; set; } = new List<LoadItem>();
    }

    public class LoadItem
    {
        public int Id { get; set; }
        public int LoadId { get; set; }
        public virtual Load Load { get; set; }

        public int? PickingListId { get; set; }
        public virtual PickingList? PickingList { get; set; }

        public int? TransferItemId { get; set; }
        public virtual TransferItem? TransferItem { get; set; }

        public int StopSequence { get; set; }
        [Precision(18, 3)]
        public decimal ShippedWeight { get; set; }
    }
}
