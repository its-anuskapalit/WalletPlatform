using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Wallet.API.Services.Interfaces;
using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;
//A regular class only exists when something calls it. A BackgroundService starts when the application starts and runs indefinitely in a background thread until the application shuts down

namespace Wallet.API.Events.Consumers;

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
        stoppingToken.ThrowIfCancellationRequested();

        var factory = new ConnectionFactory
        {
            HostName             = _config["RabbitMQ:HostName"]!,
            UserName             = _config["RabbitMQ:UserName"]!,
            Password             = _config["RabbitMQ:Password"]!,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel    = _connection.CreateModel();

        // Declare the queue (idempotent — safe to call even if already exists)
        _channel.QueueDeclare(
            queue:      RabbitMQQueues.UserRegistered,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", $"{RabbitMQQueues.UserRegistered}.dlx" }
            });

        // Only fetch 1 message at a time — process before getting another
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceived;

        _channel.BasicConsume(
            queue:       RabbitMQQueues.UserRegistered,
            autoAck:     false,  // Manual ack — we confirm only after successful processing
            consumer:    consumer);

        _logger.LogInformation("UserRegisteredConsumer started, listening on queue: {Queue}",
            RabbitMQQueues.UserRegistered);

        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs args)
    {
        var body    = Encoding.UTF8.GetString(args.Body.ToArray());
        var retries = 0;

        try
        {
            var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (@event is null)
                throw new InvalidOperationException("Failed to deserialize UserRegisteredEvent.");

            // Use a scope because IWalletService is Scoped, but this consumer is Singleton
            using var scope = _serviceProvider.CreateScope();
            var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

            await walletService.CreateWalletAsync(@event.UserId);

            // Acknowledge — message removed from queue
            _channel!.BasicAck(args.DeliveryTag, multiple: false);

            _logger.LogInformation("Wallet created for registered user {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserRegisteredEvent. Body: {Body}", body);

            // Requeue up to 3 times, then send to dead-letter queue
            var requeued = (args.Redelivered == false);
            _channel!.BasicNack(args.DeliveryTag, multiple: false, requeue: requeued);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}