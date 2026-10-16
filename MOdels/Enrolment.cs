using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        [MaxLength(50)]
        public string? PaymentReference { get; set; }

        // Navigation properties
        [ForeignKey("EventID")]
        public Event Event { get; set; } = null!;

        [ForeignKey("ParticipantID")]
        public Participant Participant { get; set; } = null!;

        [ForeignKey("CategoryID")]
        public Category Category { get; set; } = null!;

        public Result? Result { get; set; }
    }
}
