using System.Collections.Generic;

namespace CMetalsWS.Models
{
    public class MachinePullingStatusDto
    {
        public string MachineName { get; set; } = default!;
        public int TotalAssignedItems { get; set; }
        public decimal TotalAssignedWeight { get; set; }
        public List<NowPlayingDto> InProgressOrders { get; set; } = new List<NowPlayingDto>();
        public NowPlayingDto? LastCompletedOrder { get; set; }
    }
}
