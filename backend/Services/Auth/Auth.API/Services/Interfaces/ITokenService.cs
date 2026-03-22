using Auth.API.Entities;

namespace Auth.API.Services.Interfaces;

public interface ITokenService
{
    string        GenerateAccessToken(User user);
    RefreshToken  GenerateRefreshToken(Guid userId, string ipAddress);
    Guid?         GetUserIdFromToken(string token);
}