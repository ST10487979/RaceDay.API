using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Helpers;
using RaceDay.API.Models;
using RaceDay.API.Services.Interfaces;

namespace RaceDay.API.Services.Implementations
{
        public class AuthService : IAuthService
        {
            private readonly RaceDayDbContext _context;

            public AuthService(RaceDayDbContext context)
            {
                _context = context;
            }

            public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
            {
                // Check if email exists
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                    return null;

                // Validate role
                if (dto.Role != "Organiser" && dto.Role != "Participant")
                    return null;

                // Create User
                var user = new User
                {
                    Email = dto.Email,
                    PasswordHash = PasswordHasher.HashPassword(dto.Password),
                    FullName = dto.FullName,
                    Role = dto.Role,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create role-specific record
                if (dto.Role == "Organiser")
                {
                    _context.Organisers.Add(new Organiser
                    {
                        UserID = user.UserID,
                        CompanyName = dto.CompanyName,
                        ContactPhone = dto.ContactPhone
                    });
                }
                else
                {
                    _context.Participants.Add(new Participant
                    {
                        UserID = user.UserID,
                        DateOfBirth = dto.DateOfBirth ?? DateTime.Now,
                        Province = dto.Province,
                        EmergencyContact = dto.EmergencyContact
                    });
                }

                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    UserID = user.UserID,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    Message = "Registration successful"
                };
            }

            public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null) return null;

                if (!PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                    return null;

                return new AuthResponseDto
                {
                    UserID = user.UserID,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    Message = "Login successful"
                };
            }
        }
}