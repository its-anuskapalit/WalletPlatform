using Catalog.API.DTOs.Request;
using Catalog.API.DTOs.Response;
using Catalog.API.Entities;
using Catalog.API.Enums;
using Catalog.API.Events.Publishers;
using Catalog.API.Repositories.Interfaces;
using Catalog.API.Services.Interfaces;
using WalletPlatform.Shared.Events;

namespace Catalog.API.Services.Implementations;

public class RedemptionService : IRedemptionService
{
    private readonly IRedemptionRepository  _redemptionRepo;
    private readonly ICatalogRepository     _catalogRepo;
    private readonly CatalogEventPublisher  _eventPublisher;
    private readonly ILogger<RedemptionService> _logger;

    public RedemptionService(
        IRedemptionRepository redemptionRepo,
        ICatalogRepository catalogRepo,
        CatalogEventPublisher eventPublisher,
        ILogger<RedemptionService> logger)
    {
        _redemptionRepo = redemptionRepo;
        _catalogRepo    = catalogRepo;
        _eventPublisher = eventPublisher;
        _logger         = logger;
    }

    public async Task<RedemptionResponseDto> RedeemAsync(Guid userId, RedeemItemRequestDto dto)
    {
        // ── Step 1: Validate catalog item ──────────────────────────────────
        var item = await _catalogRepo.GetByIdAsync(dto.CatalogItemId)
            ?? throw new KeyNotFoundException("Catalog item not found.");

        if (!item.IsActive)
            throw new InvalidOperationException("This item is no longer available.");

        if (item.ValidUntil.HasValue && item.ValidUntil < DateTime.UtcNow)
            throw new InvalidOperationException("This offer has expired.");

        if (item.StockCount == 0)
            throw new InvalidOperationException("This item is out of stock.");

        // ── Step 2: Create redemption record (Pending) ─────────────────────
        var redemption = new Redemption
        {
            UserId        = userId,
            CatalogItemId = item.Id,
            PointsSpent   = item.PointsCost,
            Status        = RedemptionStatus.Pending
        };

        var created = await _redemptionRepo.CreateAsync(redemption);

        // ── Step 3: Decrement stock if limited ────────────────────────────
        if (item.StockCount > 0)
        {
            item.StockCount--;
            await _catalogRepo.UpdateAsync(item);
        }

        // ── Step 4: Publish RedemptionRequestedEvent ──────────────────────
        // Rewards service will deduct points on receiving this
        _eventPublisher.PublishRedemptionRequested(new RedemptionRequestedEvent
        {
            UserId     = userId,
            ItemId     = item.Id,
            ItemName   = item.Name,
            PointsCost = item.PointsCost
        });

        // ── Step 5: Mark Processing ────────────────────────────────────────
        created.Status = RedemptionStatus.Processing;
        await _redemptionRepo.UpdateAsync(created);

        _logger.LogInformation(
            "Redemption initiated: {RedemptionId} | User: {UserId} | Item: {ItemName}",
            created.Id, userId, item.Name);

        return MapToResponse(created, item.Name);
    }

    public async Task<List<RedemptionResponseDto>> GetUserRedemptionsAsync(
        Guid userId, int page, int pageSize)
    {
        var redemptions = await _redemptionRepo.GetByUserIdAsync(userId, page, pageSize);
        return redemptions
            .Select(r => MapToResponse(r, r.CatalogItem.Name))
            .ToList();
    }

    public async Task<RedemptionResponseDto> GetByIdAsync(Guid redemptionId)
    {
        var redemption = await _redemptionRepo.GetByIdAsync(redemptionId)
            ?? throw new KeyNotFoundException("Redemption not found.");

        return MapToResponse(redemption, redemption.CatalogItem.Name);
    }

    public async Task CompleteRedemptionAsync(Guid redemptionId)
    {
        var redemption = await _redemptionRepo.GetByIdAsync(redemptionId)
            ?? throw new KeyNotFoundException("Redemption not found.");

        redemption.Status      = RedemptionStatus.Completed;
        redemption.CompletedAt = DateTime.UtcNow;
        redemption.VoucherCode = GenerateVoucherCode();

        await _redemptionRepo.UpdateAsync(redemption);

        _logger.LogInformation(
            "Redemption completed: {RedemptionId} | Voucher: {Code}",
            redemptionId, redemption.VoucherCode);
    }

    public async Task FailRedemptionAsync(Guid redemptionId, string reason)
    {
        var redemption = await _redemptionRepo.GetByIdAsync(redemptionId)
            ?? throw new KeyNotFoundException("Redemption not found.");

        // Restore stock if limited item
        var item = await _catalogRepo.GetByIdAsync(redemption.CatalogItemId);
        if (item is not null && item.StockCount >= 0)
        {
            item.StockCount++;
            await _catalogRepo.UpdateAsync(item);
        }

        redemption.Status        = RedemptionStatus.Failed;
        redemption.FailureReason = reason;
        await _redemptionRepo.UpdateAsync(redemption);

        _logger.LogWarning(
            "Redemption failed: {RedemptionId} | Reason: {Reason}",
            redemptionId, reason);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private static string GenerateVoucherCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var code = new char[12];
        for (var i = 0; i < code.Length; i++)
            code[i] = chars[Random.Shared.Next(chars.Length)];

        // Format: XXXX-XXXX-XXXX
        return $"{new string(code[..4])}-{new string(code[4..8])}-{new string(code[8..12])}";
    }

    private static RedemptionResponseDto MapToResponse(Redemption r, string itemName) => new()
    {
        Id            = r.Id,
        UserId        = r.UserId,
        ItemName      = itemName,
        PointsSpent   = r.PointsSpent,
        Status        = r.Status.ToString(),
        VoucherCode   = r.VoucherCode,
        FailureReason = r.FailureReason,
        CreatedAt     = r.CreatedAt,
        CompletedAt   = r.CompletedAt
    };
}