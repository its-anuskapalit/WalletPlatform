namespace Auth.API.Entities;

public class UserProfile
{
    public Guid    Id          { get; set; } = Guid.NewGuid();
    public Guid    UserId      { get; set; }
    public string  FirstName   { get; set; } = string.Empty;
    public string  LastName    { get; set; } = string.Empty;
    public string? AvatarUrl   { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address     { get; set; }
    public string? City        { get; set; }
    public string? State       { get; set; }
    public string? PinCode     { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}