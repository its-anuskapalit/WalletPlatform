namespace Notification.API.DTOs.Response;

public class NotificationLogResponseDto
{
    public Guid     Id            { get; set; }
    public string   Type          { get; set; } = string.Empty;
    public string   Channel       { get; set; } = string.Empty;
    public string   Status        { get; set; } = string.Empty;
    public string   Subject       { get; set; } = string.Empty;
    public string?  FailureReason { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime? SentAt       { get; set; }
}