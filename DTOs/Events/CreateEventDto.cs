using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Event
{
    public class CreateEventDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public int MaxEntries { get; set; }

        [Required]
        public decimal Distance { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty; // run, walk, cycle
    }
}
