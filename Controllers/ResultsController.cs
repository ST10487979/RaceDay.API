using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Result;
using RaceDay.API.Helpers;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class ResultsController : ControllerBase
        {
            private readonly RaceDayDbContext _context;

            public ResultsController(RaceDayDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Capture result (Organiser only)
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> Create([FromBody] CreateResultDto dto)
            {
                var role = HttpContext.Session.GetString("Role");
                var userId = HttpContext.Session.GetObject<int?>("UserID");

                if (userId == null) return Unauthorized(new { error = "Authentication required" });
                if (role != "Organiser") return StatusCode(403, new { error = "Organiser role required" });

                var enrolment = await _context.Enrolments
                    .Include(e => e.Event)
                    .FirstOrDefaultAsync(e => e.EnrolmentID == dto.EnrolmentID);

                if (enrolment == null) return NotFound(new { error = "Enrolment not found" });

                var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
                if (organiser == null || enrolment.Event.OrganiserID != organiser.OrganiserID)
                    return StatusCode(403, new { error = "You can only capture results for your own events" });

                TimeSpan? finishTime = null;
                if (!string.IsNullOrEmpty(dto.FinishTime))
                    finishTime = TimeSpan.Parse(dto.FinishTime);

                var result = new Result
                {
                    EnrolmentID = dto.EnrolmentID,
                    FinishTime = finishTime,
                    Position = dto.Position,
                    DNF = dto.DNF,
                    Notes = dto.Notes,
                    RecordedAt = DateTime.Now
                };

                _context.Results.Add(result);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Result captured", result.ResultID });
            }

            /// <summary>
            /// Get results for event (public)
            /// </summary>
            [HttpGet("event/{eventId}")]
            public async Task<IActionResult> GetByEvent(int eventId)
            {
                var results = await _context.Results
                    .Include(r => r.Enrolment).ThenInclude(e => e.Participant).ThenInclude(p => p.User)
                    .Include(r => r.Enrolment).ThenInclude(e => e.Category)
                    .Where(r => r.Enrolment.EventID == eventId)
                    .Select(r => new
                    {
                        r.ResultID,
                        ParticipantName = r.Enrolment.Participant.User.FullName,
                        CategoryName = r.Enrolment.Category.Name,
                        r.FinishTime,
                        r.Position,
                        r.DNF
                    })
                    .ToListAsync();

                return Ok(results);
            }

            
            [HttpGet("my")]
            public async Task<IActionResult> GetMyResults()
            {
                var role = HttpContext.Session.GetString("Role");
                var userId = HttpContext.Session.GetObject<int?>("UserID");

                if (userId == null) return Unauthorized(new { error = "Authentication required" });
                if (role != "Participant") return StatusCode(403, new { error = "Participant role required" });

                var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
                if (participant == null) return Ok(new List<object>());

                var results = await _context.Results
                    .Include(r => r.Enrolment).ThenInclude(e => e.Event)
                    .Include(r => r.Enrolment).ThenInclude(e => e.Category)
                    .Where(r => r.Enrolment.ParticipantID == participant.ParticipantID)
                    .Select(r => new
                    {
                        r.ResultID,
                        EventTitle = r.Enrolment.Event.Title,
                        EventDate = r.Enrolment.Event.EventDate,
                        CategoryName = r.Enrolment.Category.Name,
                        r.FinishTime,
                        r.Position,
                        r.DNF
                    })
                    .ToListAsync();

                return Ok(results);
            }
        }
}
