using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Organiser? Organiser { get; set; }
        public Participant? Participant { get; set; }
    }
}

