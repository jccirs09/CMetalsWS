namespace CMetalsWS.Data
{
    public class ItemRelationship
    {
        public int Id { get; set; }
        public string ParentItemId { get; set; } = default!;
        public string ChildItemId { get; set; } = default!;
        public string ParentItemDescription { get; set; } = string.Empty;
        public string ChildItemDescription { get; set; } = string.Empty;
        public string Relation { get; set; } = "CoilToSheet";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
