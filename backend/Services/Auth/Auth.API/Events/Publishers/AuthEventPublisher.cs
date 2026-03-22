using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;
using WalletPlatform.Shared.Messaging;

namespace Auth.API.Events.Publishers;

public class AuthEventPublisher
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<AuthEventPublisher> _logger;

    public AuthEventPublisher(IRabbitMQPublisher publisher, ILogger<AuthEventPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public void PublishUserRegistered(UserRegisteredEvent @event)
    {
        try
        {
            _publisher.Publish(@event, RabbitMQQueues.UserRegistered);
            _logger.LogInformation("Published UserRegisteredEvent for {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserRegisteredEvent for {UserId}", @event.UserId);
        }
    }

    public void PublishKYCApproved(KYCApprovedEvent @event)
    {
        try
        {
            _publisher.Publish(@event, RabbitMQQueues.KYCApproved);
            _logger.LogInformation("Published KYCApprovedEvent for {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish KYCApprovedEvent for {UserId}", @event.UserId);
        }
    }
}