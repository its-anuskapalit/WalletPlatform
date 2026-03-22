using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;
using WalletPlatform.Shared.Messaging;

namespace Catalog.API.Events.Publishers;

public class CatalogEventPublisher
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<CatalogEventPublisher> _logger;

    public CatalogEventPublisher(
        IRabbitMQPublisher publisher,
        ILogger<CatalogEventPublisher> logger)
    {
        _publisher = publisher;
        _logger    = logger;
    }

    public void PublishRedemptionRequested(RedemptionRequestedEvent @event)
    {
        try
        {
            _publisher.Publish(@event, RabbitMQQueues.RedemptionRequested);
            _logger.LogInformation(
                "Published RedemptionRequestedEvent for user {UserId} | Item: {ItemId}",
                @event.UserId, @event.ItemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish RedemptionRequestedEvent for user {UserId}", @event.UserId);
        }
    }
}