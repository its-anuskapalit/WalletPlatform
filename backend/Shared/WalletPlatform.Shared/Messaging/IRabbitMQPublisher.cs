//Without the interface, unit tests would require a running RabbitMQ instance. With the interface, tests run anywhere with no infrastructure dependencies.
namespace WalletPlatform.Shared.Messaging;
public interface IRabbitMQPublisher
{
    void Publish<T>(T message, string queueName) where T : class;
}