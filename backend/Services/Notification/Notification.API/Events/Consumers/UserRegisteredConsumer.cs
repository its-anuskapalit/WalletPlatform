using System.Text;
using System.Text.Json;
using Notification.API.Enums;
using Notification.API.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;

namespace Notification.API.Events.Consumers;

public class UserRegisteredConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRegisteredConsumer> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel?      _channel;

    public UserRegisteredConsumer(
        IServiceProvider serviceProvider,
        ILogger<UserRegisteredConsumer> logger,
        IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _logger          = logger;
        _config          = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName             = _config["RabbitMQ:HostName"]!,
            UserName             = _config["RabbitMQ:UserName"]!,
            Password             = _config["RabbitMQ:Password"]!,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel    = _connection.CreateModel();

        // Unique queue per service — same pattern as Rewards
        var queueName = $"{RabbitMQQueues.UserRegistered}.notifications";

        _channel.QueueDeclare(
            queue:      queueName,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", $"{queueName}.dlx" }
            });

        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceived;
        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("Notification UserRegisteredConsumer started");
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.ToArray());
        try
        {
            var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Deserialization failed.");

            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider
                .GetRequiredService<INotificationService>();

            await notificationService.SendAsync(
                userId:          @event.UserId,
                recipientEmail:  @event.Email,
                recipientPhone:  @event.PhoneNumber,
                fullName:        @event.FullName,
                type:            NotificationType.Welcome,
                placeholders:    new Dictionary<string, string>());

            _channel!.BasicAck(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Notification UserRegisteredConsumer");
            _channel!.BasicNack(args.DeliveryTag, false, requeue: !args.Redelivered);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}