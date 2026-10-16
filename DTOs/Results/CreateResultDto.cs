using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Result
{
    public class CreateResultDto
    {
        [Required]
        public int EnrolmentID { get; set; }

        public string? FinishTime { get; set; } // Format: "HH:mm:ss"

        public int? Position { get; set; }

        public bool DNF { get; set; } = false;

        public string? Notes { get; set; }
    }
}
