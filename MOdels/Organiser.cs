using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Organiser
    {
        [Key]
        public int OrganiserID { get; set; }

        [Required]
        public int UserID { get; set; }

        [MaxLength(100)]
        public string? CompanyName { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public User User { get; set; } = null!;

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
