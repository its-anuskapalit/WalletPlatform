using Notification.API.Enums;

namespace Notification.API.Entities;

public class NotificationTemplate
{
    public Guid                Id       { get; set; } = Guid.NewGuid();
    public NotificationType    Type     { get; set; }
    public NotificationChannel Channel  { get; set; }
    public string              Subject  { get; set; } = string.Empty;
    public string              Body     { get; set; } = string.Empty;
    public bool                IsActive { get; set; } = true;
    public DateTime            CreatedAt { get; set; } = DateTime.UtcNow;
}