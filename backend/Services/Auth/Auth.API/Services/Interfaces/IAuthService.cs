using Auth.API.DTOs.Request;
using Auth.API.DTOs.Response;

namespace Auth.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, string ipAddress);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, string ipAddress);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task                  LogoutAsync(string refreshToken);
    Task<UserResponseDto> GetProfileAsync(Guid userId);
}