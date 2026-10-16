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
        public class EventsController : ControllerBase
        {
            private readonly RaceDayDbContext _context;

            public EventsController(RaceDayDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Get all events (public)
            /// </summary>
            [HttpGet]
            public async Task<IActionResult> GetAll()
            {
                var events = await _context.Events
                    .Include(e => e.Organiser).ThenInclude(o => o.User)
                    .Include(e => e.Categories)
                    .Select(e => new
                    {
                        e.EventID,
                        e.Title,
                        e.Description,
                        e.EventDate,
                        e.Location,
                        e.MaxEntries,
                        e.Status,
                        OrganiserName = e.Organiser.User.FullName,
                        CategoryCount = e.Categories.Count
                    })
                    .ToListAsync();

                return Ok(events);
            }

            /// <summary>
            /// Get event by ID (public)
            /// </summary>
            [HttpGet("{id}")]
            public async Task<IActionResult> GetById(int id)
            {
                var evt = await _context.Events
                    .Include(e => e.Organiser).ThenInclude(o => o.User)
                    .Include(e => e.Categories)
                    .FirstOrDefaultAsync(e => e.EventID == id);

                if (evt == null)
                    return NotFound(new { error = "Event not found" });

                return Ok(new
                {
                    evt.EventID,
                    evt.Title,
                    evt.Description,
                    evt.EventDate,
                    evt.Location,
                    evt.MaxEntries,
                    evt.Status,
                    OrganiserName = evt.Organiser.User.FullName,
                    Categories = evt.Categories.Select(c => new
                    {
                        c.CategoryID,
                        c.Name,
                        c.Distance,
                        c.EntryFee,
                        c.MaxCapacity
                    })
                });
            }

            /// <summary>
            /// Create event (Organiser only)
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
            {
                var role = HttpContext.Session.GetString("Role");
                var userId = HttpContext.Session.GetObject<int?>("UserID");

                if (userId == null)
                    return Unauthorized(new { error = "Authentication required" });

                if (role != "Organiser")
                    return StatusCode(403, new { error = "Organiser role required" });

                var organiser = await _context.Organisers
                    .FirstOrDefaultAsync(o => o.UserID == userId);

                if (organiser == null)
                    return StatusCode(403, new { error = "Organiser profile not found" });

                var evt = new Event
                {
                    OrganiserID = organiser.OrganiserID,
                    Title = dto.Title,
                    Description = dto.Description,
                    EventDate = dto.EventDate,
                    Location = dto.Location,
                    MaxEntries = dto.MaxEntries,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                };

                _context.Events.Add(evt);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = evt.EventID }, new
                {
                    evt.EventID,
                    evt.Title,
                    evt.EventDate,
                    evt.Location,
                    evt.Status
                });
            }

            /// summary
            /// Update event (Organiser only, must own event)
            /// summary
            [HttpPut("{id}")]
            public async Task<IActionResult> Update(int id, [FromBody] CreateEventDto dto)
            {
                var role = HttpContext.Session.GetString("Role");
                var userId = HttpContext.Session.GetObject<int?>("UserID");

                if (userId == null) return Unauthorized(new { error = "Authentication required" });
                if (role != "Organiser") return StatusCode(403, new { error = "Organiser role required" });

                var evt = await _context.Events
                    .Include(e => e.Organiser)
                    .FirstOrDefaultAsync(e => e.EventID == id);

                if (evt == null) return NotFound(new { error = "Event not found" });

                var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
                if (organiser == null || evt.OrganiserID != organiser.OrganiserID)
                    return StatusCode(403, new { error = "You can only edit your own events" });

                evt.Title = dto.Title;
                evt.Description = dto.Description;
                evt.EventDate = dto.EventDate;
                evt.Location = dto.Location;
                evt.MaxEntries = dto.MaxEntries;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Event updated", evt.EventID, evt.Title });
            }

            /// <summary>
            /// Delete event (Organiser only, must own event)
            /// </summary>
            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(int id)
            {
                var role = HttpContext.Session.GetString("Role");
                var userId = HttpContext.Session.GetObject<int?>("UserID");

                if (userId == null) return Unauthorized(new { error = "Authentication required" });
                if (role != "Organiser") return StatusCode(403, new { error = "Organiser role required" });

                var evt = await _context.Events.FindAsync(id);
                if (evt == null) return NotFound(new { error = "Event not found" });

                var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
                if (organiser == null || evt.OrganiserID != organiser.OrganiserID)
                    return StatusCode(403, new { error = "You can only delete your own events" });

                _context.Events.Remove(evt);
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
}
