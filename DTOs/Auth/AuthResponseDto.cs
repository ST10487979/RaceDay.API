namespace RaceDay.API.DTOs.Auth
{
    public class AuthResponseDto
    {
        public int UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
