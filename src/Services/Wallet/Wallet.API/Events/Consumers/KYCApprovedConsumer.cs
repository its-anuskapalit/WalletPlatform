using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Wallet.API.Services.Interfaces;
using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;

namespace Wallet.API.Events.Consumers;

public class KYCApprovedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KYCApprovedConsumer> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel?      _channel;

    public KYCApprovedConsumer(
        IServiceProvider serviceProvider,
        ILogger<KYCApprovedConsumer> logger,
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

        _channel.QueueDeclare(
            queue:      RabbitMQQueues.KYCApproved,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", $"{RabbitMQQueues.KYCApproved}.dlx" }
            });

        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceived;

        _channel.BasicConsume(
            queue:    RabbitMQQueues.KYCApproved,
            autoAck:  false,
            consumer: consumer);

        _logger.LogInformation("KYCApprovedConsumer started");
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.ToArray());

        try
        {
            var @event = JsonSerializer.Deserialize<KYCApprovedEvent>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (@event is null)
                throw new InvalidOperationException("Failed to deserialize KYCApprovedEvent.");

            using var scope = _serviceProvider.CreateScope();
            var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

            await walletService.ActivateWalletAsync(@event.UserId);

            _channel!.BasicAck(args.DeliveryTag, false);

            _logger.LogInformation("Wallet activated for user {UserId} after KYC approval",
                @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing KYCApprovedEvent");
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