using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class ItemRelationship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemCode { get; set; } = default!;
        public string Description { get; set; } = string.Empty;
        public string? CoilRelationship { get; set; }
    }
}
