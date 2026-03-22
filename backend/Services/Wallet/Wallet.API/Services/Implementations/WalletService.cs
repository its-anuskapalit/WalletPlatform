using Wallet.API.DTOs.Request;
using Wallet.API.DTOs.Response;
using Wallet.API.Entities;
using Wallet.API.Enums;
using Wallet.API.Events.Publishers;
using Wallet.API.Repositories.Interfaces;
using Wallet.API.Services.Interfaces;
using WalletPlatform.Shared.Events;

namespace Wallet.API.Services.Implementations;

public class WalletService : IWalletService
{
    private readonly IWalletRepository     _walletRepo;
    private readonly WalletEventPublisher  _eventPublisher;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IWalletRepository walletRepo,
        WalletEventPublisher eventPublisher,
        ILogger<WalletService> logger)
    {
        _walletRepo     = walletRepo;
        _eventPublisher = eventPublisher;
        _logger         = logger;
    }

    public async Task<WalletResponseDto> GetWalletAsync(Guid userId)
    {
        var wallet = await _walletRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Wallet not found for this user.");

        return MapToResponse(wallet);
    }

    public async Task<WalletResponseDto> CreateWalletAsync(Guid userId)
    {
        // Guard — wallet already exists (idempotent creation)
        var existing = await _walletRepo.GetByUserIdAsync(userId);
        if (existing is not null)
        {
            _logger.LogWarning("Wallet already exists for user {UserId}", userId);
            return MapToResponse(existing);
        }

        var wallet = new Wallet.API.Entities.Wallet
        {
            UserId       = userId,
            WalletNumber = GenerateWalletNumber(),
            Balance      = 0.00m,
            Currency     = "INR",
            Status       = WalletStatus.Pending  // Active only after KYC
        };

        var created = await _walletRepo.CreateAsync(wallet);
        _logger.LogInformation("Wallet created for user {UserId} | WalletNo: {WalletNumber}",
            userId, created.WalletNumber);

        return MapToResponse(created);
    }

    public async Task ActivateWalletAsync(Guid userId)
    {
        var wallet = await _walletRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        if (wallet.Status == WalletStatus.Active) return;

        wallet.Status = WalletStatus.Active;
        await _walletRepo.UpdateAsync(wallet);

        _logger.LogInformation("Wallet activated for user {UserId}", userId);
    }

    public async Task<WalletResponseDto> FundWalletAsync(Guid userId, FundWalletRequestDto dto)
    {
        if (dto.Amount <= 0)
            throw new InvalidOperationException("Fund amount must be greater than zero.");

        var wallet = await _walletRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        EnsureWalletOperable(wallet);

        wallet.Balance += dto.Amount;
        var updated = await _walletRepo.UpdateAsync(wallet);

        _eventPublisher.PublishWalletFunded(new WalletFundedEvent
        {
            UserId   = userId,
            WalletId = wallet.Id,
            Amount   = dto.Amount,
            Currency = wallet.Currency
        });

        _logger.LogInformation("Wallet funded: {UserId} | Amount: {Amount}", userId, dto.Amount);
        return MapToResponse(updated);
    }

    public async Task<WalletResponseDto> WithdrawAsync(Guid userId, WithdrawRequestDto dto)
    {
        if (dto.Amount <= 0)
            throw new InvalidOperationException("Withdrawal amount must be greater than zero.");

        var wallet = await _walletRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        EnsureWalletOperable(wallet);

        if (wallet.AvailableBalance < dto.Amount)
            throw new InvalidOperationException(
                $"Insufficient balance. Available: {wallet.AvailableBalance:F2} {wallet.Currency}");

        wallet.Balance -= dto.Amount;
        var updated = await _walletRepo.UpdateAsync(wallet);

        _logger.LogInformation("Withdrawal: {UserId} | Amount: {Amount}", userId, dto.Amount);
        return MapToResponse(updated);
    }

    public async Task<WalletResponseDto> FreezeWalletAsync(
        Guid walletId, Guid adminId, FreezeWalletRequestDto dto)
    {
        var wallet = await _walletRepo.GetByIdAsync(walletId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        if (wallet.Status == WalletStatus.Frozen)
            throw new InvalidOperationException("Wallet is already frozen.");

        wallet.Status = WalletStatus.Frozen;
        await _walletRepo.UpdateAsync(wallet);

        await _walletRepo.AddFreezeLogAsync(new WalletFreezeLog
        {
            WalletId = walletId,
            ActionBy = adminId,
            Action   = "FREEZE",
            Reason   = dto.Reason
        });

        _logger.LogInformation("Wallet frozen: {WalletId} by admin {AdminId}", walletId, adminId);
        return MapToResponse(wallet);
    }

    public async Task<WalletResponseDto> UnfreezeWalletAsync(
        Guid walletId, Guid adminId, FreezeWalletRequestDto dto)
    {
        var wallet = await _walletRepo.GetByIdAsync(walletId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        if (wallet.Status != WalletStatus.Frozen)
            throw new InvalidOperationException("Wallet is not frozen.");

        wallet.Status = WalletStatus.Active;
        await _walletRepo.UpdateAsync(wallet);

        await _walletRepo.AddFreezeLogAsync(new WalletFreezeLog
        {
            WalletId = walletId,
            ActionBy = adminId,
            Action   = "UNFREEZE",
            Reason   = dto.Reason
        });

        _logger.LogInformation("Wallet unfrozen: {WalletId} by admin {AdminId}", walletId, adminId);
        return MapToResponse(wallet);
    }

    public async Task DebitAsync(Guid userId, decimal amount)
    {
        var wallet = await _walletRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        EnsureWalletOperable(wallet);

        if (wallet.AvailableBalance < amount)
            throw new InvalidOperationException("Insufficient balance for debit.");

        wallet.Balance -= amount;
        await _walletRepo.UpdateAsync(wallet);

        _logger.LogInformation("Wallet debited: {UserId} | Amount: {Amount}", userId, amount);
    }

    public async Task CreditAsync(Guid userId, decimal amount)
    {
        var wallet = await _walletRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Wallet not found.");

        wallet.Balance += amount;
        await _walletRepo.UpdateAsync(wallet);

        _logger.LogInformation("Wallet credited: {UserId} | Amount: {Amount}", userId, amount);
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private static void EnsureWalletOperable(Wallet.API.Entities.Wallet wallet)
    {
        if (wallet.Status == WalletStatus.Pending)
            throw new InvalidOperationException(
                "Wallet is pending KYC approval. Please complete your KYC first.");

        if (wallet.Status == WalletStatus.Frozen)
            throw new InvalidOperationException(
                "Wallet is frozen. Please contact support.");

        if (wallet.Status == WalletStatus.Closed)
            throw new InvalidOperationException("Wallet is closed.");
    }

    private static string GenerateWalletNumber()
    {
        // Format: WLT + timestamp digits + 4 random digits
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var random    = Random.Shared.Next(1000, 9999).ToString();
        return $"WLT{timestamp[^8..]}{random}";
    }

    private static WalletResponseDto MapToResponse(Wallet.API.Entities.Wallet w) => new()
    {
        Id               = w.Id,
        UserId           = w.UserId,
        WalletNumber     = w.WalletNumber,
        Balance          = w.Balance,
        FrozenAmount     = w.FrozenAmount,
        AvailableBalance = w.AvailableBalance,
        Currency         = w.Currency,
        Status           = w.Status.ToString(),
        CreatedAt        = w.CreatedAt,
        PaymentMethods   = w.PaymentMethods.Select(p => new PaymentMethodResponseDto
        {
            Id          = p.Id,
            Type        = p.Type.ToString(),
            DisplayName = p.DisplayName,
            Last4Digits = p.Last4Digits,
            BankName    = p.BankName,
            UpiId       = p.UpiId,
            IsDefault   = p.IsDefault
        }).ToList()
    };
}