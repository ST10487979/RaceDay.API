using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Helpers;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
 
  
        [ApiController]
        [Route("api/[controller]")]
        public class CategoriesController : ControllerBase
        {
            private readonly RaceDayDbContext _context;

            public CategoriesController(RaceDayDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Get all categories for an event (public)
            /// </summary>
            [HttpGet("event/{eventId}")]
            public async Task<IActionResult> GetByEvent(int eventId)
            {
                var categories = await _context.Categories
                    .Where(c => c.EventID == eventId)
                    .Select(c => new
                    {
                        c.CategoryID,
                        c.EventID,
                        c.Name,
                        c.Distance,
                        c.EntryFee,
                        c.MaxCapacity,
                        CurrentEnrolments = c.Enrolments.Count
                    })
                    .ToListAsync();

                return Ok(categories);
            }

            /// <summary>
            /// Create category (Organiser only, must own event)
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> Create([FromBody] Category category)
            {
                var role = HttpContext.Session.GetString("Role");
                var userId = HttpContext.Session.GetObject<int?>("UserID");

                if (userId == null) return Unauthorized(new { error = "Authentication required" });
                if (role != "Organiser") return StatusCode(403, new { error = "Organiser role required" });

                var evt = await _context.Events.FindAsync(category.EventID);
                if (evt == null) return NotFound(new { error = "Event not found" });

                var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
                if (organiser == null || evt.OrganiserID != organiser.OrganiserID)
                    return StatusCode(403, new { error = "You can only add categories to your own events" });

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByEvent), new { eventId = category.EventID }, category);
            }
        }
}
