using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Distance { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal EntryFee { get; set; }

        [Required]
        public int MaxCapacity { get; set; }

        // Navigation properties
        [ForeignKey("EventID")]
        public Event Event { get; set; } = null!;

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
