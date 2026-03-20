namespace WalletPlatform.Shared.Messaging;

public interface IRabbitMQPublisher
{
    void Publish<T>(T message, string queueName) where T : class;
}