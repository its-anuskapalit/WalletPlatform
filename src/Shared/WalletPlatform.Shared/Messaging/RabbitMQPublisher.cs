using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using WalletPlatform.Shared.Messaging;

namespace WalletPlatform.Shared.Messaging;

public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQPublisher(string hostName = "localhost")
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = "guest",
            Password = "guest",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel    = _connection.CreateModel();
    }

    public void Publish<T>(T message, string queueName) where T : class
    {
        _channel.QueueDeclare(
            queue:      queueName,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                // Dead-letter exchange wired up for every queue
                { "x-dead-letter-exchange", $"{queueName}.dlx" }
            });

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel.CreateBasicProperties();
        props.Persistent  = true;
        props.ContentType = "application/json";

        _channel.BasicPublish(
            exchange:   string.Empty,
            routingKey: queueName,
            basicProperties: props,
            body:       body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}