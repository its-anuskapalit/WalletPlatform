using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rewards.API.Services.Interfaces;
using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;

namespace Rewards.API.Events.Consumers;

public class RedemptionRequestedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RedemptionRequestedConsumer> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel?      _channel;

    public RedemptionRequestedConsumer(
        IServiceProvider serviceProvider,
        ILogger<RedemptionRequestedConsumer> logger,
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

        _channel.QueueDeclare(
            queue:      RabbitMQQueues.RedemptionRequested,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", $"{RabbitMQQueues.RedemptionRequested}.dlx" }
            });

        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceived;

        _channel.BasicConsume(
            queue:    RabbitMQQueues.RedemptionRequested,
            autoAck:  false,
            consumer: consumer);

        _logger.LogInformation("RedemptionRequestedConsumer started in Rewards service");
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.ToArray());
        try
        {
            var @event = JsonSerializer.Deserialize<RedemptionRequestedEvent>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Deserialization failed.");

            using var scope = _serviceProvider.CreateScope();
            var rewardsService = scope.ServiceProvider.GetRequiredService<IRewardsService>();

            await rewardsService.DeductPointsAsync(
                userId:       @event.UserId,
                points:       @event.PointsCost,
                redemptionId: @event.ItemId,
                itemName:     @event.ItemName);

            _channel!.BasicAck(args.DeliveryTag, false);

            _logger.LogInformation(
                "Points deducted | User: {UserId} | Points: {Points}",
                @event.UserId, @event.PointsCost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RedemptionRequestedEvent");
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