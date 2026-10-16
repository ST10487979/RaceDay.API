using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Organiser or Participant

        // Participant-specific (optional for organisers)
        public DateTime? DateOfBirth { get; set; }
        public string? Province { get; set; }
        public string? EmergencyContact { get; set; }

        // Organiser-specific (optional for participants)
        public string? CompanyName { get; set; }
        public string? ContactPhone { get; set; }
    }
}
