using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Event;
using RaceDay.API.Helpers;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EventsControllerTests
    {
        private RaceDayDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new RaceDayDbContext(options);

            // Seed
            context.Users.Add(new User { UserID = 1, Email = "org@test.com", FullName = "Org", Role = "Organiser", PasswordHash = "x" });
            context.Users.Add(new User { UserID = 2, Email = "part@test.com", FullName = "Part", Role = "Participant", PasswordHash = "x" });
            context.Organisers.Add(new Organiser { OrganiserID = 1, UserID = 1 });
            context.Participants.Add(new Participant { ParticipantID = 1, UserID = 2, DateOfBirth = DateTime.Now });
            context.SaveChanges();
            return context;
        }

        private ControllerContext GetControllerContext(int? userId, string? role)
        {
            var httpContext = new DefaultHttpContext();
            var session = new Mock<ISession>();

            if (userId.HasValue)
            {
                var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(userId.Value);
                session.Setup(s => s.TryGetValue("UserID", out bytes)).Returns(true);
            }

            if (role != null)
            {
                var roleBytes = System.Text.Encoding.UTF8.GetBytes(role);
                session.Setup(s => s.TryGetValue("Role", out roleBytes)).Returns(true);
            }

            httpContext.Session = session.Object;
            return new ControllerContext { HttpContext = httpContext };
        }

        [Fact]
        public async Task Create_AsOrganiser_ReturnsCreated()
        {
            var context = GetContext();
            var controller = new EventsController(context)
            {
                ControllerContext = GetControllerContext(1, "Organiser")
            };

            var dto = new CreateEventDto
            {
                Title = "Test Event",
                EventDate = DateTime.Now.AddDays(30),
                Location = "Test Location",
                MaxEntries = 100,
                Distance = 10,
                EventType = "run"
            };

            var result = await controller.Create(dto);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Create_AsParticipant_ReturnsForbidden()
        {
            var context = GetContext();
            var controller = new EventsController(context)
            {
                ControllerContext = GetControllerContext(2, "Participant")
            };

            var dto = new CreateEventDto
            {
                Title = "Test",
                EventDate = DateTime.Now.AddDays(1),
                Location = "Loc",
                MaxEntries = 50,
                Distance = 5,
                EventType = "run"
            };

            var result = await controller.Create(dto);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusResult.StatusCode);
        }
    }
}