using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Helpers;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetObject<int?>("UserID");
            if (userId == null) return Unauthorized(new { error = "Authentication required" });

            var user = await _context.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) return NotFound(new { error = "User not found" });

            if (user.Role == "Organiser" && user.Organiser != null)
            {
                return Ok(new
                {
                    user.UserID,
                    user.Email,
                    user.FullName,
                    user.Role,
                    user.Organiser.CompanyName,
                    user.Organiser.ContactPhone
                });
            }

            return Ok(new
            {
                user.UserID,
                user.Email,
                user.FullName,
                user.Role,
                user.Participant?.DateOfBirth,
                user.Participant?.Province,
                user.Participant?.EmergencyContact
            });
        }

        /// <summary>
        /// Update current user profile
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetObject<int?>("UserID");
            if (userId == null) return Unauthorized(new { error = "Authentication required" });

            var user = await _context.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) return NotFound(new { error = "User not found" });

            if (!string.IsNullOrEmpty(dto.FullName))
                user.FullName = dto.FullName;

            if (user.Role == "Participant" && user.Participant != null)
            {
                if (dto.Province != null) user.Participant.Province = dto.Province;
                if (dto.EmergencyContact != null) user.Participant.EmergencyContact = dto.EmergencyContact;
                if (dto.DateOfBirth.HasValue) user.Participant.DateOfBirth = dto.DateOfBirth.Value;
            }

            if (user.Role == "Organiser" && user.Organiser != null)
            {
                if (dto.CompanyName != null) user.Organiser.CompanyName = dto.CompanyName;
                if (dto.ContactPhone != null) user.Organiser.ContactPhone = dto.ContactPhone;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }
    }

    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? Province { get; set; }
        public string? EmergencyContact { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactPhone { get; set; }
    }

}
