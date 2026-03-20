using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rewards.API.Services.Interfaces;
using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;

namespace Rewards.API.Events.Consumers;

public class TransactionCompletedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionCompletedConsumer> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel?      _channel;

    public TransactionCompletedConsumer(
        IServiceProvider serviceProvider,
        ILogger<TransactionCompletedConsumer> logger,
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

        // Rewards gets its own queue — Notification also consumes the same event
        var queueName = $"{RabbitMQQueues.TransactionCompleted}.rewards";

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

        _logger.LogInformation("TransactionCompletedConsumer (Rewards) started");
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.ToArray());
        try
        {
            var @event = JsonSerializer.Deserialize<TransactionCompletedEvent>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Deserialization failed.");

            using var scope = _serviceProvider.CreateScope();
            var rewardsService = scope.ServiceProvider.GetRequiredService<IRewardsService>();

            // Award points to sender only — not recipient
            await rewardsService.AwardPointsAsync(
                userId:          @event.UserId,
                amount:          @event.Amount,
                transactionType: "PeerTransfer",  // default — extend later for merchant pay
                transactionId:   @event.TransactionId);

            _channel!.BasicAck(args.DeliveryTag, false);

            _logger.LogInformation(
                "Points processed for transaction {TxnId}", @event.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TransactionCompletedEvent in Rewards");
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