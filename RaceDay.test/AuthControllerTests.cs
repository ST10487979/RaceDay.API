using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Services.Implementations;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task Register_ValidParticipant_ReturnsSuccess()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new AuthService(context);
            var dto = new RegisterDto
            {
                Email = "test@test.com",
                Password = "Password123",
                FullName = "Test User",
                Role = "Participant",
                DateOfBirth = new DateTime(1990, 1, 1),
                Province = "Gauteng",
                EmergencyContact = "Contact"
            };

            // Act
            var result = await service.RegisterAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@test.com", result.Email);
            Assert.Equal("Participant", result.Role);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsNull()
        {
            var context = GetInMemoryContext();
            var service = new AuthService(context);
            var dto = new RegisterDto
            {
                Email = "dup@test.com",
                Password = "Password123",
                FullName = "Test",
                Role = "Participant",
                DateOfBirth = DateTime.Now
            };

            await service.RegisterAsync(dto);
            var result = await service.RegisterAsync(dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsUser()
        {
            var context = GetInMemoryContext();
            var service = new AuthService(context);
            await service.RegisterAsync(new RegisterDto
            {
                Email = "login@test.com",
                Password = "Password123",
                FullName = "Test",
                Role = "Participant",
                DateOfBirth = DateTime.Now
            });

            var result = await service.LoginAsync(new LoginDto
            {
                Email = "login@test.com",
                Password = "Password123"
            });

            Assert.NotNull(result);
            Assert.Equal("login@test.com", result.Email);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsNull()
        {
            var context = GetInMemoryContext();
            var service = new AuthService(context);
            await service.RegisterAsync(new RegisterDto
            {
                Email = "login2@test.com",
                Password = "Password123",
                FullName = "Test",
                Role = "Participant",
                DateOfBirth = DateTime.Now
            });

            var result = await service.LoginAsync(new LoginDto
            {
                Email = "login2@test.com",
                Password = "WrongPassword"
            });

            Assert.Null(result);
        }
    }
}
