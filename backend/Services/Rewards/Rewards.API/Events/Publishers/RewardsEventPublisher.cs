using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;
using WalletPlatform.Shared.Messaging;

namespace Rewards.API.Events.Publishers;

public class RewardsEventPublisher
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<RewardsEventPublisher> _logger;

    public RewardsEventPublisher(
        IRabbitMQPublisher publisher,
        ILogger<RewardsEventPublisher> logger)
    {
        _publisher = publisher;
        _logger    = logger;
    }

    public void PublishPointsAwarded(PointsAwardedEvent @event)
    {
        try
        {
            _publisher.Publish(@event, RabbitMQQueues.PointsAwarded);
            _logger.LogInformation(
                "Published PointsAwardedEvent: {Points} pts for user {UserId}",
                @event.PointsAwarded, @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish PointsAwardedEvent for user {UserId}",
                @event.UserId);
        }
    }
}