using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(50)]
        public string? Province { get; set; }

        [MaxLength(100)]
        public string? EmergencyContact { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public User User { get; set; } = null!;

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
