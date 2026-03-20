using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;
using WalletPlatform.Shared.Messaging;

namespace Transaction.API.Events.Publishers;

public class TransactionEventPublisher
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<TransactionEventPublisher> _logger;

    public TransactionEventPublisher(
        IRabbitMQPublisher publisher,
        ILogger<TransactionEventPublisher> logger)
    {
        _publisher = publisher;
        _logger    = logger;
    }

    public void PublishTransactionCompleted(TransactionCompletedEvent @event)
    {
        try
        {
            _publisher.Publish(@event, RabbitMQQueues.TransactionCompleted);
            _logger.LogInformation(
                "Published TransactionCompletedEvent: {TxnId}", @event.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish TransactionCompletedEvent: {TxnId}", @event.TransactionId);
        }
    }

    public void PublishTransactionFailed(TransactionFailedEvent @event)
    {
        try
        {
            _publisher.Publish(@event, RabbitMQQueues.TransactionFailed);
            _logger.LogInformation(
                "Published TransactionFailedEvent: {TxnId}", @event.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish TransactionFailedEvent: {TxnId}", @event.TransactionId);
        }
    }
}