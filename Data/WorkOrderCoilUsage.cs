using System;
using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    public class WorkOrderCoilUsage
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        public virtual WorkOrder? WorkOrder { get; set; }

        public int Sequence { get; set; }

        [MaxLength(128)]
        public string CoilInventoryId { get; set; } = default!;

        [MaxLength(64)]
        public string CoilTagNumber { get; set; } = default!;

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        [MaxLength(256)]
        public string? Reason { get; set; }
    }
}
