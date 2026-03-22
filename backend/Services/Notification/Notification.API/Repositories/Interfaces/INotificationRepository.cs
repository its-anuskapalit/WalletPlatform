using Notification.API.Entities;
using Notification.API.Enums;

namespace Notification.API.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<NotificationTemplate?> GetTemplateAsync(
        NotificationType type, NotificationChannel channel);

    Task<NotificationLog>           CreateLogAsync(NotificationLog log);
    Task<NotificationLog>           UpdateLogAsync(NotificationLog log);
    Task<List<NotificationLog>>     GetByUserIdAsync(Guid userId, int page, int pageSize);
    Task<NotificationLog?>          GetByIdAsync(Guid id);
}