using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        public bool DNF { get; set; } = false;

        public string? Notes { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("EnrolmentID")]
        public Enrolment Enrolment { get; set; } = null!;
    }
}
