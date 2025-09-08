using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class CityCentroid
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string City { get; set; } = default!;

        [Required, MaxLength(64)]
        public string Province { get; set; } = default!;

        [Required]
        [Column(TypeName = "decimal(9, 6)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(9, 6)")]
        public decimal Longitude { get; set; }
    }
}
