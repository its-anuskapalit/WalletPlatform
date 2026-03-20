namespace Auth.API.Entities;

public class AuditLog
{
    public Guid     Id         { get; set; } = Guid.NewGuid();
    public Guid?    UserId     { get; set; }
    public string   Action     { get; set; } = string.Empty;
    public string   Resource   { get; set; } = string.Empty;
    public string?  IpAddress  { get; set; }
    public string?  UserAgent  { get; set; }
    public bool     Success    { get; set; }
    public string?  Details    { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
}