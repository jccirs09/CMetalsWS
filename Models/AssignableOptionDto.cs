namespace CMetalsWS.Models
{
    public class AssignableOptionDto
    {
        public string Name { get; set; }
        public string Type { get; set; } // "Machine" or "Pulling"
        public int? MachineId { get; set; }
        public byte? BuildingCategory { get; set; }
    }
}
