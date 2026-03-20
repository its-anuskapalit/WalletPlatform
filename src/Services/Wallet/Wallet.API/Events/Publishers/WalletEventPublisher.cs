using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;
using WalletPlatform.Shared.Messaging;

namespace Wallet.API.Events.Publishers;

public class WalletEventPublisher
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<WalletEventPublisher> _logger;

    public WalletEventPublisher(
        IRabbitMQPublisher publisher,
        ILogger<WalletEventPublisher> logger)
    {
        _publisher = publisher;
        _logger    = logger;
    }

    public void PublishWalletFunded(WalletFundedEvent @event)
    {
        try
        {
            _publisher.Publish(@event, RabbitMQQueues.WalletFunded);
            _logger.LogInformation("Published WalletFundedEvent for {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish WalletFundedEvent");
        }
    }
}