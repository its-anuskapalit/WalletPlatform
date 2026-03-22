namespace Auth.API.DTOs.Response;
//sended from server to frontend
public class AuthResponseDto
{
    public string AccessToken  { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt  { get; set; }
    public UserResponseDto User { get; set; } = null!;
}