// ============================================================
// FILE: backend\Services\Wallet\Wallet.API.Tests\Services\WalletServiceTests.cs
// ============================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Wallet.API.DTOs.Request;
using Wallet.API.Entities;
using Wallet.API.Enums;
using Wallet.API.Events.Publishers;
using Wallet.API.Repositories.Interfaces;
using Wallet.API.Services.Implementations;
using WalletPlatform.Shared.Messaging;

namespace Wallet.API.Tests.Services;

public class WalletServiceTests
{
    private readonly Mock<IWalletRepository>      _walletRepo     = new();
    private readonly Mock<WalletEventPublisher>   _eventPublisher;
    private readonly Mock<ILogger<WalletService>> _logger         = new();
    private readonly WalletService                _sut;

    public WalletServiceTests()
    {
        _eventPublisher = new Mock<WalletEventPublisher>(
            new Mock<IRabbitMQPublisher>().Object,
            new Mock<ILogger<WalletEventPublisher>>().Object);

        _sut = new WalletService(
            _walletRepo.Object,
            _eventPublisher.Object,
            _logger.Object);
    }

    private static Wallet.API.Entities.Wallet ActiveWallet(Guid userId) => new()
    {
        Id           = Guid.NewGuid(),
        UserId       = userId,
        WalletNumber = "WLT123456789",
        Balance      = 2000m,
        FrozenAmount = 0m,
        Currency     = "INR",
        Status       = WalletStatus.Active
    };

    // ═════════════════════════════════════════════════════════════════════
    // CreateWalletAsync Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateWalletAsync_CreatesWalletWithPendingStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Wallet.API.Entities.Wallet? saved = null;

        _walletRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Wallet.API.Entities.Wallet?)null);
        _walletRepo.Setup(r => r.CreateAsync(It.IsAny<Wallet.API.Entities.Wallet>()))
            .Callback<Wallet.API.Entities.Wallet>(w => saved = w)
            .ReturnsAsync((Wallet.API.Entities.Wallet w) => w);

        // Act
        await _sut.CreateWalletAsync(userId);

        // Assert
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(WalletStatus.Pending);
        saved.UserId.Should().Be(userId);
        saved.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task CreateWalletAsync_WalletNumberStartsWithWLT()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Wallet.API.Entities.Wallet? saved = null;

        _walletRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Wallet.API.Entities.Wallet?)null);
        _walletRepo.Setup(r => r.CreateAsync(It.IsAny<Wallet.API.Entities.Wallet>()))
            .Callback<Wallet.API.Entities.Wallet>(w => saved = w)
            .ReturnsAsync((Wallet.API.Entities.Wallet w) => w);

        // Act
        await _sut.CreateWalletAsync(userId);

        // Assert
        saved!.WalletNumber.Should().StartWith("WLT");
    }

    [Fact]
    public async Task CreateWalletAsync_WhenWalletAlreadyExists_DoesNotCreateDuplicate()
    {
        // Arrange
        var userId         = Guid.NewGuid();
        var existingWallet = ActiveWallet(userId);

        _walletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingWallet);

        // Act
        await _sut.CreateWalletAsync(userId);

        // Assert
        _walletRepo.Verify(r => r.CreateAsync(It.IsAny<Wallet.API.Entities.Wallet>()), Times.Never);
    }

    // ═════════════════════════════════════════════════════════════════════
    // ActivateWalletAsync Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ActivateWalletAsync_ChangesStatusToActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new Wallet.API.Entities.Wallet
        {
            Id     = Guid.NewGuid(),
            UserId = userId,
            Status = WalletStatus.Pending,
            Balance = 0m, FrozenAmount = 0m, Currency = "INR",
            WalletNumber = "WLT123"
        };

        _walletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
        _walletRepo.Setup(r => r.UpdateAsync(It.IsAny<Wallet.API.Entities.Wallet>()))
            .ReturnsAsync((Wallet.API.Entities.Wallet w) => w);

        // Act
        await _sut.ActivateWalletAsync(userId);

        // Assert
        _walletRepo.Verify(r => r.UpdateAsync(It.Is<Wallet.API.Entities.Wallet>(
            w => w.Status == WalletStatus.Active)), Times.Once);
    }

    // ═════════════════════════════════════════════════════════════════════
    // FundWalletAsync Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FundWalletAsync_IncreasesBalanceByAmount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = ActiveWallet(userId);
        var dto    = new FundWalletRequestDto { Amount = 500m, Description = "Top up" };

        _walletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
        _walletRepo.Setup(r => r.UpdateAsync(It.IsAny<Wallet.API.Entities.Wallet>()))
            .ReturnsAsync((Wallet.API.Entities.Wallet w) => w);

        // Act
        var result = await _sut.FundWalletAsync(userId, dto);

        // Assert
        _walletRepo.Verify(r => r.UpdateAsync(
            It.Is<Wallet.API.Entities.Wallet>(w => w.Balance == 2500m)), Times.Once);
    }

    [Fact]
    public async Task FundWalletAsync_PublishesWalletFundedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = ActiveWallet(userId);
        var dto    = new FundWalletRequestDto { Amount = 500m, Description = "Top up" };

        _walletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
        _walletRepo.Setup(r => r.UpdateAsync(It.IsAny<Wallet.API.Entities.Wallet>()))
            .ReturnsAsync((Wallet.API.Entities.Wallet w) => w);

        // Act
        await _sut.FundWalletAsync(userId, dto);

        // Assert
        _eventPublisher.Verify(
            e => e.PublishWalletFunded(It.Is<WalletPlatform.Shared.Events.WalletFundedEvent>(
                ev => ev.UserId == userId && ev.Amount == 500m)),
            Times.Once);
    }

    [Fact]
    public async Task FundWalletAsync_WhenWalletPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new Wallet.API.Entities.Wallet
        {
            UserId = userId, Status = WalletStatus.Pending,
            Balance = 0, FrozenAmount = 0, Currency = "INR", WalletNumber = "WLT123"
        };
        var dto = new FundWalletRequestDto { Amount = 500m };

        _walletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

        // Act
        Func<Task> act = () => _sut.FundWalletAsync(userId, dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*pending*");
    }

    [Fact]
    public async Task FundWalletAsync_WhenWalletFrozen_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new Wallet.API.Entities.Wallet
        {
            UserId = userId, Status = WalletStatus.Frozen,
            Balance = 1000, FrozenAmount = 0, Currency = "INR", WalletNumber = "WLT123"
        };
        var dto = new FundWalletRequestDto { Amount = 500m };

        _walletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

        // Act
        Func<Task> act = () => _sut.FundWalletAsync(userId, dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*frozen*");
    }

    [Fact]
    public async Task FundWalletAsync_WhenWalletNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto    = new FundWalletRequestDto { Amount = 500m };

        _walletRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Wallet.API.Entities.Wallet?)null);

        // Act
        Func<Task> act = () => _sut.FundWalletAsync(userId, dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ═════════════════════════════════════════════════════════════════════
    // AvailableBalance Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public void AvailableBalance_IsBalanceMinusFrozenAmount()
    {
        // Arrange
        var wallet = new Wallet.API.Entities.Wallet
        {
            Balance      = 2000m,
            FrozenAmount = 300m
        };

        // Act & Assert
        wallet.AvailableBalance.Should().Be(1700m);
    }

    [Fact]
    public void AvailableBalance_WhenNoFrozenAmount_EqualsBalance()
    {
        var wallet = new Wallet.API.Entities.Wallet
        {
            Balance      = 1500m,
            FrozenAmount = 0m
        };
        wallet.AvailableBalance.Should().Be(1500m);
    }
}


// ============================================================
// FILE: backend\Services\Transaction\Transaction.API.Tests\Services\TransactionServiceTests.cs
// ============================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Transaction.API.DTOs.Request;
using Transaction.API.Entities;
using Transaction.API.Enums;
using Transaction.API.Events.Publishers;
using Transaction.API.Repositories.Interfaces;
using Transaction.API.Services.Implementations;
using WalletPlatform.Shared.Events;
using WalletPlatform.Shared.Messaging;

namespace Transaction.API.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository>      _txnRepo        = new();
    private readonly Mock<ILedgerRepository>           _ledgerRepo     = new();
    private readonly Mock<TransactionEventPublisher>   _eventPublisher;
    private readonly Mock<ILogger<TransactionService>> _logger         = new();
    private readonly TransactionService                _sut;

    public TransactionServiceTests()
    {
        _eventPublisher = new Mock<TransactionEventPublisher>(
            new Mock<IRabbitMQPublisher>().Object,
            new Mock<ILogger<TransactionEventPublisher>>().Object);

        _sut = new TransactionService(
            _txnRepo.Object,
            _ledgerRepo.Object,
            _eventPublisher.Object,
            _logger.Object);
    }

    private static InitiatePaymentRequestDto ValidPaymentDto(Guid recipientId) => new()
    {
        RecipientId = recipientId,
        Amount      = 500m,
        Description = "Dinner split"
    };

    // ═════════════════════════════════════════════════════════════════════
    // Idempotency Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task InitiatePaymentAsync_WithExistingIdempotencyKey_ReturnsCachedResult()
    {
        // Arrange
        var senderId       = Guid.NewGuid();
        var recipientId    = Guid.NewGuid();
        var idempotencyKey = "test-key-001";
        var dto            = ValidPaymentDto(recipientId);

        var existingRecord = new IdempotencyRecord
        {
            IdempotencyKey = idempotencyKey,
            UserId         = senderId,
            TransactionId  = Guid.NewGuid()
        };
        var existingTxn = new Transaction.API.Entities.Transaction
        {
            Id          = existingRecord.TransactionId,
            SenderId    = senderId,
            RecipientId = recipientId,
            Amount      = 500m,
            Status      = TransactionStatus.Completed,
            LedgerEntries = new List<LedgerEntry>()
        };

        _txnRepo.Setup(r => r.GetIdempotencyRecordAsync(idempotencyKey, senderId))
            .ReturnsAsync(existingRecord);
        _txnRepo.Setup(r => r.GetByIdAsync(existingRecord.TransactionId))
            .ReturnsAsync(existingTxn);

        // Act
        var result = await _sut.InitiatePaymentAsync(senderId, idempotencyKey, dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingRecord.TransactionId);

        // No new transaction created
        _txnRepo.Verify(r => r.CreateAsync(It.IsAny<Transaction.API.Entities.Transaction>()),
            Times.Never);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithSameKeyTwice_DoesNotDoubleCharge()
    {
        // Arrange
        var senderId       = Guid.NewGuid();
        var recipientId    = Guid.NewGuid();
        var idempotencyKey = "unique-key-001";
        var dto            = ValidPaymentDto(recipientId);

        var record = new IdempotencyRecord
        {
            IdempotencyKey = idempotencyKey,
            UserId         = senderId,
            TransactionId  = Guid.NewGuid()
        };
        var txn = new Transaction.API.Entities.Transaction
        {
            Id            = record.TransactionId,
            Amount        = 500m,
            Status        = TransactionStatus.Completed,
            LedgerEntries = new List<LedgerEntry>()
        };

        _txnRepo.Setup(r => r.GetIdempotencyRecordAsync(idempotencyKey, senderId))
            .ReturnsAsync(record);
        _txnRepo.Setup(r => r.GetByIdAsync(record.TransactionId)).ReturnsAsync(txn);

        // Act
        await _sut.InitiatePaymentAsync(senderId, idempotencyKey, dto);
        await _sut.InitiatePaymentAsync(senderId, idempotencyKey, dto);

        // Assert — ledger written zero times (both calls returned cache)
        _ledgerRepo.Verify(r => r.CreateManyAsync(It.IsAny<IEnumerable<LedgerEntry>>()),
            Times.Never);
    }

    // ═════════════════════════════════════════════════════════════════════
    // Validation Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task InitiatePaymentAsync_SelfPayment_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto    = new InitiatePaymentRequestDto
        {
            RecipientId = userId,   // same as sender
            Amount      = 100m,
            Description = "Self"
        };

        _txnRepo.Setup(r => r.GetIdempotencyRecordAsync(It.IsAny<string>(), userId))
            .ReturnsAsync((IdempotencyRecord?)null);

        // Act
        Func<Task> act = () => _sut.InitiatePaymentAsync(userId, "key-001", dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*yourself*");
    }

    [Fact]
    public async Task InitiatePaymentAsync_ZeroAmount_ThrowsInvalidOperationException()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = new InitiatePaymentRequestDto
        {
            RecipientId = recipientId,
            Amount      = 0m,
            Description = "Zero payment"
        };

        _txnRepo.Setup(r => r.GetIdempotencyRecordAsync(It.IsAny<string>(), senderId))
            .ReturnsAsync((IdempotencyRecord?)null);

        // Act
        Func<Task> act = () => _sut.InitiatePaymentAsync(senderId, "key-002", dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public async Task InitiatePaymentAsync_NegativeAmount_ThrowsInvalidOperationException()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = new InitiatePaymentRequestDto
        {
            RecipientId = recipientId,
            Amount      = -100m,
            Description = "Negative"
        };

        _txnRepo.Setup(r => r.GetIdempotencyRecordAsync(It.IsAny<string>(), senderId))
            .ReturnsAsync((IdempotencyRecord?)null);

        // Act
        Func<Task> act = () => _sut.InitiatePaymentAsync(senderId, "key-003", dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ═════════════════════════════════════════════════════════════════════
    // Happy Path Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task InitiatePaymentAsync_ValidPayment_WritesExactlyTwoLedgerEntries()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = ValidPaymentDto(recipientId);

        IEnumerable<LedgerEntry>? writtenEntries = null;

        SetupHappyPath(senderId, recipientId);

        _ledgerRepo.Setup(r => r.CreateManyAsync(It.IsAny<IEnumerable<LedgerEntry>>()))
            .Callback<IEnumerable<LedgerEntry>>(entries => writtenEntries = entries)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.InitiatePaymentAsync(senderId, "key-happy-001", dto);

        // Assert
        writtenEntries.Should().NotBeNull();
        writtenEntries.Should().HaveCount(2);
    }

    [Fact]
    public async Task InitiatePaymentAsync_ValidPayment_OneDebitOneCreditEntry()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = ValidPaymentDto(recipientId);

        IEnumerable<LedgerEntry>? writtenEntries = null;

        SetupHappyPath(senderId, recipientId);

        _ledgerRepo.Setup(r => r.CreateManyAsync(It.IsAny<IEnumerable<LedgerEntry>>()))
            .Callback<IEnumerable<LedgerEntry>>(entries => writtenEntries = entries)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.InitiatePaymentAsync(senderId, "key-happy-002", dto);

        // Assert
        writtenEntries.Should().Contain(e =>
            e.AccountId == senderId && e.EntryType == LedgerEntryType.Debit);
        writtenEntries.Should().Contain(e =>
            e.AccountId == recipientId && e.EntryType == LedgerEntryType.Credit);
    }

    [Fact]
    public async Task InitiatePaymentAsync_ValidPayment_SetsStatusToCompleted()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = ValidPaymentDto(recipientId);

        SetupHappyPath(senderId, recipientId);
        _ledgerRepo.Setup(r => r.CreateManyAsync(It.IsAny<IEnumerable<LedgerEntry>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.InitiatePaymentAsync(senderId, "key-happy-003", dto);

        // Assert
        _txnRepo.Verify(r => r.UpdateStatusAsync(
            It.IsAny<Guid>(), TransactionStatus.Completed, null), Times.Once);
    }

    [Fact]
    public async Task InitiatePaymentAsync_ValidPayment_PublishesCompletedEvent()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = ValidPaymentDto(recipientId);

        SetupHappyPath(senderId, recipientId);
        _ledgerRepo.Setup(r => r.CreateManyAsync(It.IsAny<IEnumerable<LedgerEntry>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.InitiatePaymentAsync(senderId, "key-happy-004", dto);

        // Assert
        _eventPublisher.Verify(
            e => e.PublishTransactionCompleted(
                It.Is<TransactionCompletedEvent>(ev => ev.UserId == senderId && ev.Amount == 500m)),
            Times.Once);
    }

    [Fact]
    public async Task InitiatePaymentAsync_ValidPayment_SavesIdempotencyRecordBeforeLedger()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = ValidPaymentDto(recipientId);
        var callOrder   = new List<string>();

        SetupHappyPath(senderId, recipientId);

        _txnRepo.Setup(r => r.SaveIdempotencyRecordAsync(It.IsAny<IdempotencyRecord>()))
            .Callback(() => callOrder.Add("idempotency"))
            .Returns(Task.CompletedTask);

        _ledgerRepo.Setup(r => r.CreateManyAsync(It.IsAny<IEnumerable<LedgerEntry>>()))
            .Callback(() => callOrder.Add("ledger"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.InitiatePaymentAsync(senderId, "key-order-001", dto);

        // Assert — idempotency MUST be saved before ledger
        callOrder.IndexOf("idempotency").Should().BeLessThan(callOrder.IndexOf("ledger"));
    }

    // ═════════════════════════════════════════════════════════════════════
    // Saga Compensation Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task InitiatePaymentAsync_WhenLedgerFails_SetsStatusToFailed()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = ValidPaymentDto(recipientId);

        SetupHappyPath(senderId, recipientId);
        _ledgerRepo.Setup(r => r.CreateManyAsync(It.IsAny<IEnumerable<LedgerEntry>>()))
            .ThrowsAsync(new Exception("DB timeout"));

        // Act
        try { await _sut.InitiatePaymentAsync(senderId, "key-fail-001", dto); } catch { }

        // Assert
        _txnRepo.Verify(r => r.UpdateStatusAsync(
            It.IsAny<Guid>(), TransactionStatus.Failed, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenLedgerFails_PublishesTransactionFailedEvent()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = ValidPaymentDto(recipientId);

        SetupHappyPath(senderId, recipientId);
        _ledgerRepo.Setup(r => r.CreateManyAsync(It.IsAny<IEnumerable<LedgerEntry>>()))
            .ThrowsAsync(new Exception("DB timeout"));

        // Act
        try { await _sut.InitiatePaymentAsync(senderId, "key-fail-002", dto); } catch { }

        // Assert
        _eventPublisher.Verify(
            e => e.PublishTransactionFailed(
                It.Is<TransactionFailedEvent>(ev => ev.SenderId == senderId && ev.Amount == 500m)),
            Times.Once);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenLedgerFails_RethrowsException()
    {
        // Arrange
        var senderId    = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto         = ValidPaymentDto(recipientId);

        SetupHappyPath(senderId, recipientId);
        _ledgerRepo.Setup(r => r.CreateManyAsync(It.IsAny<IEnumerable<LedgerEntry>>()))
            .ThrowsAsync(new Exception("DB timeout"));

        // Act
        Func<Task> act = () => _sut.InitiatePaymentAsync(senderId, "key-fail-003", dto);

        // Assert — exception bubbles up to controller / middleware
        await act.Should().ThrowAsync<Exception>().WithMessage("DB timeout");
    }

    // ═════════════════════════════════════════════════════════════════════
    // Balance Derived Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetDerivedBalanceAsync_ReturnsCreditMinusDebit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _ledgerRepo.Setup(r => r.GetAccountBalanceAsync(userId)).ReturnsAsync(1500m);

        // Act
        var balance = await _sut.GetDerivedBalanceAsync(userId);

        // Assert
        balance.Should().Be(1500m);
    }

    // ── Private setup helper ─────────────────────────────────────────────
    private void SetupHappyPath(Guid senderId, Guid recipientId)
    {
        var txnId = Guid.NewGuid();
        var txn   = new Transaction.API.Entities.Transaction
        {
            Id            = txnId,
            SenderId      = senderId,
            RecipientId   = recipientId,
            Amount        = 500m,
            Status        = TransactionStatus.Pending,
            LedgerEntries = new List<LedgerEntry>()
        };

        _txnRepo.Setup(r => r.GetIdempotencyRecordAsync(It.IsAny<string>(), senderId))
            .ReturnsAsync((IdempotencyRecord?)null);
        _txnRepo.Setup(r => r.CreateAsync(It.IsAny<Transaction.API.Entities.Transaction>()))
            .ReturnsAsync(txn);
        _txnRepo.Setup(r => r.SaveIdempotencyRecordAsync(It.IsAny<IdempotencyRecord>()))
            .Returns(Task.CompletedTask);
        _txnRepo.Setup(r => r.UpdateStatusAsync(It.IsAny<Guid>(),
            It.IsAny<TransactionStatus>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _ledgerRepo.Setup(r => r.GetAccountBalanceAsync(senderId)).ReturnsAsync(2000m);
        _ledgerRepo.Setup(r => r.GetAccountBalanceAsync(recipientId)).ReturnsAsync(500m);
    }
}
