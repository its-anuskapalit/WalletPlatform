using Notification.API.DTOs.Response;
using Notification.API.Enums;

namespace Notification.API.Services.Interfaces;

public interface INotificationService
{
    Task SendAsync(
        Guid   userId,
        string recipientEmail,
        string recipientPhone,
        string fullName,
        NotificationType type,
        Dictionary<string, string> placeholders);

    Task<List<NotificationLogResponseDto>> GetUserNotificationsAsync(
        Guid userId, int page, int pageSize);
}