using Auth.API.Enums;

namespace Auth.API.Entities;
public class User //User table
{
    public Guid     Id            { get; set; } = Guid.NewGuid();
    public string   Email         { get; set; } = string.Empty;
    public string   PasswordHash  { get; set; } = string.Empty;
    public string   PhoneNumber   { get; set; } = string.Empty;
    public UserRole Role          { get; set; } = UserRole.User;
    public bool     IsActive      { get; set; } = true;
    public bool     IsEmailVerified { get; set; } = false;
    public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt    { get; set; }

    // Navigation properties
    public UserProfile?        Profile       { get; set; }
    public KYCRecord?          KYCRecord     { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}