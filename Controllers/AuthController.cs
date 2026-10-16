using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Helpers;
using RaceDay.API.Services.Interfaces;


namespace RaceDay.API.Controllers
{
    
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController : ControllerBase
        {
            private readonly IAuthService _authService;

            public AuthController(IAuthService authService)
            {
                _authService = authService;
            }

            /// <summary>
            /// Register a new user (Organiser or Participant)
            /// </summary>
            [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] RegisterDto dto)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.RegisterAsync(dto);

                if (result == null)
                    return BadRequest(new { error = "Email already registered or invalid role" });

                return CreatedAtAction(nameof(Register), result);
            }

            /// <summary>
            /// Login with email and password
            /// </summary>
            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDto dto)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.LoginAsync(dto);

                if (result == null)
                    return Unauthorized(new { error = "Invalid email or password" });

                // Store in session
                HttpContext.Session.SetObject("UserID", result.UserID);
                HttpContext.Session.SetString("Role", result.Role);
                HttpContext.Session.SetString("FullName", result.FullName);

                return Ok(result);
            }

            /// <summary>
            /// Logout - clear session
            /// </summary>
            [HttpPost("logout")]
            public IActionResult Logout()
            {
                HttpContext.Session.Clear();
                return Ok(new { message = "Logged out successfully" });
            }

            /// <summary>
            /// Get current logged-in user info
            /// </summary>
            [HttpGet("me")]
            public IActionResult GetCurrentUser()
            {
                var userId = HttpContext.Session.GetObject<int?>("UserID");
                if (userId == null)
                    return Unauthorized(new { error = "Not logged in" });

                return Ok(new
                {
                    UserID = userId,
                    Role = HttpContext.Session.GetString("Role"),
                    FullName = HttpContext.Session.GetString("FullName")
                });
            }
        }
}

