using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Event;
using RaceDay.API.Helpers;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
  
        [ApiController]
        [Route("api/[controller]")]
        public class EnrolmentsController : ControllerBase
        {
            private readonly RaceDayDbContext _context;

            public EnrolmentsController(RaceDayDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Enrol participant in event (Participant only)
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentDto dto)
            {
                var role = HttpContext.Session.GetString("Role");
                var userId = HttpContext.Session.GetObject<int?>("UserID");

                if (userId == null) return Unauthorized(new { error = "Authentication required" });
                if (role != "Participant") return StatusCode(403, new { error = "Participant role required" });

                var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
                if (participant == null) return StatusCode(403, new { error = "Participant profile not found" });

                // Check if already enrolled
                var existing = await _context.Enrolments
                    .AnyAsync(e => e.EventID == dto.EventID
                        && e.ParticipantID == participant.ParticipantID
                        && e.CategoryID == dto.CategoryID);

                if (existing)
                    return Conflict(new { error = "You are already enrolled in this category" });

                // Check capacity
                var category = await _context.Categories.FindAsync(dto.CategoryID);
                if (category == null) return NotFound(new { error = "Category not found" });

                var currentCount = await _context.Enrolments.CountAsync(e => e.CategoryID == dto.CategoryID);
                if (currentCount >= category.MaxCapacity)
                    return BadRequest(new { error = "This category has reached maximum capacity" });

                var enrolment = new Enrolment
                {
                    EventID = dto.EventID,
                    ParticipantID = participant.ParticipantID,
                    CategoryID = dto.CategoryID,
                    Status = "Pending",
                    EntryDate = DateTime.Now
                };

                _context.Enrolments.Add(enrolment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMyEnrolments), new { }, new
                {
                    enrolment.EnrolmentID,
                    enrolment.EventID,
                    enrolment.CategoryID,
                    enrolment.Status,
                    enrolment.EntryDate
                });
            }

            /// <summary>
            /// Get my enrolments (Participant)
            /// </summary>
            [HttpGet("my")]
            public async Task<IActionResult> GetMyEnrolments()
            {
                var role = HttpContext.Session.GetString("Role");
                var userId = HttpContext.Session.GetObject<int?>("UserID");

                if (userId == null) return Unauthorized(new { error = "Authentication required" });
                if (role != "Participant") return StatusCode(403, new { error = "Participant role required" });

                var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
                if (participant == null) return Ok(new List<object>());

                var enrolments = await _context.Enrolments
                    .Where(e => e.ParticipantID == participant.ParticipantID)
                    .Include(e => e.Event)
                    .Include(e => e.Category)
                    .Select(e => new
                    {
                        e.EnrolmentID,
                        EventTitle = e.Event.Title,
                        EventDate = e.Event.EventDate,
                        CategoryName = e.Category.Name,
                        CategoryDistance = e.Category.Distance,
                        e.Status,
                        e.EntryDate
                    })
                    .ToListAsync();

                return Ok(enrolments);
            }

            /// <summary>
            /// Get all enrolments for an event (Organiser only)
            /// </summary>
            [HttpGet("event/{eventId}")]
            public async Task<IActionResult> GetByEvent(int eventId)
            {
                var role = HttpContext.Session.GetString("Role");
                var userId = HttpContext.Session.GetObject<int?>("UserID");

                if (userId == null) return Unauthorized(new { error = "Authentication required" });
                if (role != "Organiser") return StatusCode(403, new { error = "Organiser role required" });

                var evt = await _context.Events.FindAsync(eventId);
                if (evt == null) return NotFound(new { error = "Event not found" });

                var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
                if (organiser == null || evt.OrganiserID != organiser.OrganiserID)
                    return StatusCode(403, new { error = "You can only view enrolments for your own events" });

                var enrolments = await _context.Enrolments
                    .Where(e => e.EventID == eventId)
                    .Include(e => e.Participant).ThenInclude(p => p.User)
                    .Include(e => e.Category)
                    .Select(e => new
                    {
                        e.EnrolmentID,
                        ParticipantName = e.Participant.User.FullName,
                        CategoryName = e.Category.Name,
                        e.Status,
                        e.EntryDate
                    })
                    .ToListAsync();

                return Ok(enrolments);
            }
        }
}
