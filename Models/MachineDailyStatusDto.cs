namespace CMetalsWS.Models
{
    public class MachineDailyStatusDto
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public string MachineType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CurrentWorkOrder { get; set; }
    }
}
