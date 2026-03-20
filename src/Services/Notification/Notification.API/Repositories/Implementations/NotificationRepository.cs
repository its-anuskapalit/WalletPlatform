using Microsoft.EntityFrameworkCore;
using Notification.API.Data;
using Notification.API.Entities;
using Notification.API.Enums;
using Notification.API.Repositories.Interfaces;

namespace Notification.API.Repositories.Implementations;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplate?> GetTemplateAsync(
        NotificationType type, NotificationChannel channel) =>
        await _context.NotificationTemplates
            .FirstOrDefaultAsync(t =>
                t.Type     == type    &&
                t.Channel  == channel &&
                t.IsActive == true);

    public async Task<NotificationLog> CreateLogAsync(NotificationLog log)
    {
        _context.NotificationLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<NotificationLog> UpdateLogAsync(NotificationLog log)
    {
        _context.NotificationLogs.Update(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<List<NotificationLog>> GetByUserIdAsync(
        Guid userId, int page, int pageSize) =>
        await _context.NotificationLogs
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<NotificationLog?> GetByIdAsync(Guid id) =>
        await _context.NotificationLogs.FindAsync(id);
}