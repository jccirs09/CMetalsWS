using CMetalsWS.Data;
using System;

namespace CMetalsWS.Models
{
    public class NowPlayingDto
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public MachineCategory MachineCategory { get; set; }
        public int WorkOrderId { get; set; }
        public string? CustomerName { get; set; }
        public double Progress { get; set; }
        public string Runtime { get; set; } = string.Empty;
        public DateTime Eta { get; set; }
        public int SwapCount { get; set; }
        public WorkOrderStatus Status { get; set; }
    }
}
