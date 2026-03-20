using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Transaction.API.Entities;
using Transaction.API.Enums;
using Transaction.API.Repositories.Interfaces;
using WalletPlatform.Shared.Constants;
using WalletPlatform.Shared.Events;

namespace Transaction.API.Events.Consumers;

public class WalletFundedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WalletFundedConsumer> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel?      _channel;

    public WalletFundedConsumer(
        IServiceProvider serviceProvider,
        ILogger<WalletFundedConsumer> logger,
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
            queue:      RabbitMQQueues.WalletFunded,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", $"{RabbitMQQueues.WalletFunded}.dlx" }
            });

        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceived;

        _channel.BasicConsume(
            queue:    RabbitMQQueues.WalletFunded,
            autoAck:  false,
            consumer: consumer);

        _logger.LogInformation("WalletFundedConsumer started");
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.ToArray());

        try
        {
            var @event = JsonSerializer.Deserialize<WalletFundedEvent>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Failed to deserialize WalletFundedEvent.");

            using var scope = _serviceProvider.CreateScope();
            var txnRepo    = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var ledgerRepo = scope.ServiceProvider.GetRequiredService<ILedgerRepository>();

            // Create a Transaction record for the top-up
            var transaction = new Transaction.API.Entities.Transaction
            {
                SenderId       = @event.UserId,
                RecipientId    = @event.UserId,
                Amount         = @event.Amount,
                Currency       = @event.Currency,
                Type           = TransactionType.WalletFund,
                Status         = TransactionStatus.Completed,
                Description    = "Wallet top-up",
                IdempotencyKey = $"fund-{@event.WalletId}-{@event.OccurredAt.Ticks}",
                CompletedAt    = DateTime.UtcNow
            };

            await txnRepo.CreateAsync(transaction);

            // Write the credit ledger entry
            var currentBalance = await ledgerRepo.GetAccountBalanceAsync(@event.UserId);

            await ledgerRepo.CreateAsync(new LedgerEntry
            {
                TransactionId = transaction.Id,
                AccountId     = @event.UserId,
                EntryType     = LedgerEntryType.Credit,
                Amount        = @event.Amount,
                Currency      = @event.Currency,
                BalanceBefore = currentBalance,
                BalanceAfter  = currentBalance + @event.Amount,
                Description   = "Wallet top-up credit"
            });

            _channel!.BasicAck(args.DeliveryTag, false);

            _logger.LogInformation(
                "Wallet fund recorded in ledger: {UserId} | Amount: {Amount}",
                @event.UserId, @event.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WalletFundedEvent");
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