namespace Notification.API.Enums;

public enum NotificationStatus
{
    Pending  = 0,
    Sent     = 1,
    Failed   = 2,
    Skipped  = 3   // user opted out or no contact info
}