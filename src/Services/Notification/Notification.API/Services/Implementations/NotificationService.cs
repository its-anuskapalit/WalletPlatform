using Notification.API.DTOs.Response;
using Notification.API.Entities;
using Notification.API.Enums;
using Notification.API.Repositories.Interfaces;
using Notification.API.Services.Interfaces;

namespace Notification.API.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IEmailService           _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repo,
        IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _repo         = repo;
        _emailService = emailService;
        _logger       = logger;
    }

    public async Task SendAsync(
        Guid   userId,
        string recipientEmail,
        string recipientPhone,
        string fullName,
        NotificationType type,
        Dictionary<string, string> placeholders)
    {
        // Always try Email first, then SMS
        await TrySendChannelAsync(
            userId, recipientEmail, fullName,
            type, NotificationChannel.Email, placeholders);

        await TrySendChannelAsync(
            userId, recipientPhone, fullName,
            type, NotificationChannel.SMS, placeholders);
    }

    public async Task<List<NotificationLogResponseDto>> GetUserNotificationsAsync(
        Guid userId, int page, int pageSize)
    {
        var logs = await _repo.GetByUserIdAsync(userId, page, pageSize);
        return logs.Select(MapToResponse).ToList();
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private async Task TrySendChannelAsync(
        Guid userId,
        string recipient,
        string fullName,
        NotificationType type,
        NotificationChannel channel,
        Dictionary<string, string> placeholders)
    {
        // Skip if no recipient address for this channel
        if (string.IsNullOrWhiteSpace(recipient))
        {
            _logger.LogInformation(
                "Skipping {Channel} notification — no recipient for user {UserId}",
                channel, userId);
            return;
        }

        // Load template
        var template = await _repo.GetTemplateAsync(type, channel);
        if (template is null)
        {
            _logger.LogWarning(
                "No template found for {Type} / {Channel} — skipping",
                type, channel);
            return;
        }

        // Apply placeholders — replace {Key} with values
        var subject = ApplyPlaceholders(template.Subject, fullName, placeholders);
        var body    = ApplyPlaceholders(template.Body,    fullName, placeholders);

        // Create log entry
        var log = new NotificationLog
        {
            UserId    = userId,
            Type      = type,
            Channel   = channel,
            Status    = NotificationStatus.Pending,
            Recipient = recipient,
            Subject   = subject,
            Body      = body
        };

        await _repo.CreateLogAsync(log);

        try
        {
            bool sent;

            if (channel == NotificationChannel.Email)
            {
                sent = await _emailService.SendAsync(recipient, subject, body);
            }
            else
            {
                // SMS simulation — same pattern, swap for Twilio/AWS SNS
                _logger.LogInformation(
                    "[SMS SIMULATED] To: {Phone} | Body: {Body}", recipient, body);
                sent = true;
            }

            log.Status = sent ? NotificationStatus.Sent : NotificationStatus.Failed;
            log.SentAt = sent ? DateTime.UtcNow : null;

            if (!sent)
                log.FailureReason = "Provider returned failure response.";
        }
        catch (Exception ex)
        {
            log.Status        = NotificationStatus.Failed;
            log.FailureReason = ex.Message;
            _logger.LogError(ex,
                "Failed to send {Channel} notification to {Recipient}", channel, recipient);
        }

        await _repo.UpdateLogAsync(log);
    }

    private static string ApplyPlaceholders(
        string template,
        string fullName,
        Dictionary<string, string> placeholders)
    {
        // Always inject FullName
        var result = template.Replace("{FullName}", fullName);

        // Apply all custom placeholders
        foreach (var (key, value) in placeholders)
            result = result.Replace($"{{{key}}}", value);

        return result;
    }

    private static NotificationLogResponseDto MapToResponse(NotificationLog n) => new()
    {
        Id            = n.Id,
        Type          = n.Type.ToString(),
        Channel       = n.Channel.ToString(),
        Status        = n.Status.ToString(),
        Subject       = n.Subject,
        FailureReason = n.FailureReason,
        CreatedAt     = n.CreatedAt,
        SentAt        = n.SentAt
    };
}