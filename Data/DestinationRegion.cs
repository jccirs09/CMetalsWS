namespace CMetalsWS.Data
{
    public class DestinationRegion
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    }
}
