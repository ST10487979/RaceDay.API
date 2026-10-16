using System.Threading.Tasks;
using RaceDay.API.DTOs.Auth;

namespace RaceDay.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    }
}
