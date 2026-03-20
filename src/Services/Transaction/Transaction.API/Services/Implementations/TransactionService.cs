using System.Text.Json;
using Transaction.API.DTOs.Request;
using Transaction.API.DTOs.Response;
using Transaction.API.Entities;
using Transaction.API.Enums;
using Transaction.API.Events.Publishers;
using Transaction.API.Repositories.Interfaces;
using Transaction.API.Services.Interfaces;
using WalletPlatform.Shared.Events;

namespace Transaction.API.Services.Implementations;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository      _txnRepo;
    private readonly ILedgerRepository           _ledgerRepo;
    private readonly TransactionEventPublisher   _eventPublisher;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository txnRepo,
        ILedgerRepository ledgerRepo,
        TransactionEventPublisher eventPublisher,
        ILogger<TransactionService> logger)
    {
        _txnRepo        = txnRepo;
        _ledgerRepo     = ledgerRepo;
        _eventPublisher = eventPublisher;
        _logger         = logger;
    }

    public async Task<TransactionResponseDto> InitiatePaymentAsync(
        Guid senderId,
        string idempotencyKey,
        InitiatePaymentRequestDto dto)
    {
        // ── Step 1: Idempotency check ──────────────────────────────────────
        // If we've seen this key before, return the cached result immediately
        var existing = await _txnRepo.GetIdempotencyRecordAsync(idempotencyKey, senderId);
        if (existing is not null)
        {
            _logger.LogWarning(
                "Duplicate request detected. IdempotencyKey: {Key} UserId: {UserId}",
                idempotencyKey, senderId);

            var cached = await _txnRepo.GetByIdAsync(existing.TransactionId);
            return MapToResponse(cached!);
        }

        // ── Step 2: Validate ───────────────────────────────────────────────
        if (dto.Amount <= 0)
            throw new InvalidOperationException("Payment amount must be greater than zero.");

        if (senderId == dto.RecipientId)
            throw new InvalidOperationException("Cannot send money to yourself.");

        // ── Step 3: Create transaction record (Pending) ────────────────────
        var transaction = new Transaction.API.Entities.Transaction
        {
            SenderId       = senderId,
            RecipientId    = dto.RecipientId,
            Amount         = dto.Amount,
            Currency       = "INR",
            Type           = TransactionType.PeerTransfer,
            Status         = TransactionStatus.Pending,
            Description    = dto.Description,
            IdempotencyKey = idempotencyKey,
            ReferenceId    = dto.ReferenceId
        };

        await _txnRepo.CreateAsync(transaction);

        // ── Step 4: Save idempotency record before processing ──────────────
        // This prevents a second identical request from processing if the
        // first is still in-flight
        await _txnRepo.SaveIdempotencyRecordAsync(new IdempotencyRecord
        {
            IdempotencyKey = idempotencyKey,
            UserId         = senderId,
            TransactionId  = transaction.Id,
            ResponseJson   = string.Empty  // will be updated after completion
        });

        // ── Step 5: Update status to Processing ───────────────────────────
        await _txnRepo.UpdateStatusAsync(transaction.Id, TransactionStatus.Processing);

        try
        {
            // ── Step 6: Write double-entry ledger ──────────────────────────
            // Get current balances for snapshot
            var senderBalance    = await _ledgerRepo.GetAccountBalanceAsync(senderId);
            var recipientBalance = await _ledgerRepo.GetAccountBalanceAsync(dto.RecipientId);

            var ledgerEntries = new List<LedgerEntry>
            {
                // Debit sender
                new LedgerEntry
                {
                    TransactionId = transaction.Id,
                    AccountId     = senderId,
                    EntryType     = LedgerEntryType.Debit,
                    Amount        = dto.Amount,
                    Currency      = "INR",
                    BalanceBefore = senderBalance,
                    BalanceAfter  = senderBalance - dto.Amount,
                    Description   = $"Payment to {dto.RecipientId}: {dto.Description}"
                },
                // Credit recipient
                new LedgerEntry
                {
                    TransactionId = transaction.Id,
                    AccountId     = dto.RecipientId,
                    EntryType     = LedgerEntryType.Credit,
                    Amount        = dto.Amount,
                    Currency      = "INR",
                    BalanceBefore = recipientBalance,
                    BalanceAfter  = recipientBalance + dto.Amount,
                    Description   = $"Payment from {senderId}: {dto.Description}"
                }
            };

            // Both entries written atomically
            await _ledgerRepo.CreateManyAsync(ledgerEntries);

            // ── Step 7: Mark transaction Completed ────────────────────────
            var completed = await _txnRepo.UpdateStatusAsync(
                transaction.Id, TransactionStatus.Completed);

            // ── Step 8: Publish TransactionCompleted event ────────────────
            // Wallet Service, Rewards, Notification all consume this
            _eventPublisher.PublishTransactionCompleted(new TransactionCompletedEvent
            {
                TransactionId = transaction.Id,
                UserId        = senderId,
                Amount        = dto.Amount,
                Currency      = "INR",
                Description   = dto.Description
            });

            _logger.LogInformation(
                "Transaction completed: {TxnId} | Sender: {SenderId} | Amount: {Amount}",
                transaction.Id, senderId, dto.Amount);

            var response = MapToResponse(completed);
            return response;
        }
        catch (Exception ex)
        {
            // ── Step 9: Saga compensation — mark Failed, publish event ─────
            // Wallet Service will reverse the debit on receiving this event
            _logger.LogError(ex,
                "Transaction failed: {TxnId} | Reason: {Reason}",
                transaction.Id, ex.Message);

            await _txnRepo.UpdateStatusAsync(
                transaction.Id,
                TransactionStatus.Failed,
                ex.Message);

            _eventPublisher.PublishTransactionFailed(new TransactionFailedEvent
            {
                TransactionId = transaction.Id,
                SenderId      = senderId,
                Amount        = dto.Amount,
                Reason        = ex.Message
            });

            throw;
        }
    }

    public async Task<TransactionResponseDto> GetTransactionAsync(Guid transactionId)
    {
        var transaction = await _txnRepo.GetByIdAsync(transactionId)
            ?? throw new KeyNotFoundException($"Transaction {transactionId} not found.");

        return MapToResponse(transaction);
    }

    public async Task<List<TransactionResponseDto>> GetUserTransactionsAsync(
        Guid userId, int page, int pageSize)
    {
        var transactions = await _txnRepo.GetByUserIdAsync(userId, page, pageSize);
        return transactions.Select(MapToResponse).ToList();
    }

    public async Task HandleTransactionFailedAsync(Guid transactionId, string reason)
    {
        await _txnRepo.UpdateStatusAsync(
            transactionId, TransactionStatus.Reversed, reason);

        _logger.LogInformation(
            "Transaction reversed: {TxnId} | Reason: {Reason}",
            transactionId, reason);
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private static TransactionResponseDto MapToResponse(
        Transaction.API.Entities.Transaction t) => new()
    {
        Id            = t.Id,
        SenderId      = t.SenderId,
        RecipientId   = t.RecipientId,
        Amount        = t.Amount,
        Currency      = t.Currency,
        Type          = t.Type.ToString(),
        Status        = t.Status.ToString(),
        Description   = t.Description,
        FailureReason = t.FailureReason,
        CreatedAt     = t.CreatedAt,
        CompletedAt   = t.CompletedAt,
        LedgerEntries = t.LedgerEntries.Select(l => new LedgerEntryResponseDto
        {
            Id            = l.Id,
            AccountId     = l.AccountId,
            EntryType     = l.EntryType.ToString(),
            Amount        = l.Amount,
            BalanceBefore = l.BalanceBefore,
            BalanceAfter  = l.BalanceAfter,
            Description   = l.Description,
            CreatedAt     = l.CreatedAt
        }).ToList()
    };
}