using Notification.API.Enums;

namespace Notification.API.Entities;

public class NotificationLog
{
    public Guid                Id          { get; set; } = Guid.NewGuid();
    public Guid                UserId      { get; set; }
    public NotificationType    Type        { get; set; }
    public NotificationChannel Channel     { get; set; }
    public NotificationStatus  Status      { get; set; } = NotificationStatus.Pending;
    public string              Recipient   { get; set; } = string.Empty; // email or phone
    public string              Subject     { get; set; } = string.Empty;
    public string              Body        { get; set; } = string.Empty;
    public string?             FailureReason { get; set; }
    public string?             ExternalRef { get; set; } // provider message ID
    public DateTime            CreatedAt   { get; set; } = DateTime.UtcNow;
    public DateTime?           SentAt      { get; set; }
}