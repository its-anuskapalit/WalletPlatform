namespace Auth.API.Entities;

public class RefreshToken
{
    public Guid     Id          { get; set; } = Guid.NewGuid();
    public Guid     UserId      { get; set; }
    public string   Token       { get; set; } = string.Empty;
    public DateTime ExpiresAt   { get; set; }
    public bool     IsRevoked   { get; set; } = false;
    public string?  ReplacedBy  { get; set; }
    public string?  CreatedByIp { get; set; }
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

    public bool IsExpired  => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive   => !IsRevoked && !IsExpired;

    // Navigation
    public User User { get; set; } = null!;
}