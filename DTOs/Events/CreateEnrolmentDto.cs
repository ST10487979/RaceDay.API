using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Event
{
    public class CreateEnrolmentDto
    {
        [Required]
        public int EventID { get; set; }

        [Required]
        public int CategoryID { get; set; }
    }
}
