using Rewards.API.DTOs.Response;
using Rewards.API.Entities;
using Rewards.API.Enums;
using Rewards.API.Events.Publishers;
using Rewards.API.Repositories.Interfaces;
using Rewards.API.Services.Interfaces;
using WalletPlatform.Shared.Events;

namespace Rewards.API.Services.Implementations;

public class RewardsService : IRewardsService
{
    private readonly ILoyaltyRepository    _loyaltyRepo;
    private readonly IPointRuleRepository  _ruleRepo;
    private readonly RewardsEventPublisher _eventPublisher;
    private readonly ILogger<RewardsService> _logger;

    public RewardsService(
        ILoyaltyRepository loyaltyRepo,
        IPointRuleRepository ruleRepo,
        RewardsEventPublisher eventPublisher,
        ILogger<RewardsService> logger)
    {
        _loyaltyRepo    = loyaltyRepo;
        _ruleRepo       = ruleRepo;
        _eventPublisher = eventPublisher;
        _logger         = logger;
    }

    public async Task<LoyaltyAccountResponseDto> GetAccountAsync(Guid userId)
    {
        var account = await _loyaltyRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Loyalty account not found.");

        return await MapToResponseAsync(account);
    }

    public async Task<List<PointTransactionResponseDto>> GetHistoryAsync(
        Guid userId, int page, int pageSize)
    {
        var account = await _loyaltyRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Loyalty account not found.");

        var history = await _loyaltyRepo.GetPointHistoryAsync(account.Id, page, pageSize);

        return history.Select(t => new PointTransactionResponseDto
        {
            Id            = t.Id,
            Type          = t.Type.ToString(),
            Points        = t.Points,
            BalanceBefore = t.BalanceBefore,
            BalanceAfter  = t.BalanceAfter,
            Description   = t.Description,
            ReferenceId   = t.ReferenceId,
            CreatedAt     = t.CreatedAt
        }).ToList();
    }

    public async Task<List<RewardTierResponseDto>> GetTiersAsync()
    {
        var tiers = await _loyaltyRepo.GetAllTiersAsync();
        return tiers.Select(t => new RewardTierResponseDto
        {
            Id               = t.Id,
            Name             = t.Name,
            MinPoints        = t.MinPoints,
            MaxPoints        = t.MaxPoints,
            MultiplierFactor = t.MultiplierFactor,
            BadgeColor       = t.BadgeColor,
            DisplayOrder     = t.DisplayOrder
        }).ToList();
    }

    public async Task CreateAccountAsync(Guid userId)
    {
        // Idempotent — if account already exists, do nothing
        var existing = await _loyaltyRepo.GetByUserIdAsync(userId);
        if (existing is not null)
        {
            _logger.LogWarning("Loyalty account already exists for user {UserId}", userId);
            return;
        }

        // New accounts start on Bronze tier
        var bronzeTier = await _loyaltyRepo.GetTierForPointsAsync(0)
            ?? throw new InvalidOperationException("Bronze tier not seeded in database.");

        var account = new LoyaltyAccount
        {
            UserId         = userId,
            TierId         = bronzeTier.Id,
            TotalPoints    = 0,
            LifetimePoints = 0,
            RedeemedPoints = 0
        };

        await _loyaltyRepo.CreateAsync(account);
        _logger.LogInformation("Loyalty account created for user {UserId}", userId);
    }

    public async Task AwardPointsAsync(
        Guid userId,
        decimal amount,
        string transactionType,
        Guid transactionId)
    {
        var account = await _loyaltyRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"Loyalty account not found for user {userId}.");

        // ── Step 1: Find the applicable point rule ─────────────────────────
        if (!Enum.TryParse<TransactionTypeRef>(transactionType, out var typeRef))
        {
            _logger.LogWarning(
                "No point rule mapping for transaction type {Type} — skipping rewards",
                transactionType);
            return;
        }

        var rule = await _ruleRepo.GetActiveRuleAsync(typeRef);
        if (rule is null)
        {
            _logger.LogInformation(
                "No active point rule for transaction type {Type} — no points awarded",
                transactionType);
            return;
        }

        // ── Step 2: Check minimum amount threshold ─────────────────────────
        if (rule.MinAmount.HasValue && amount < rule.MinAmount.Value)
        {
            _logger.LogInformation(
                "Amount {Amount} below minimum {Min} for rule {Rule} — no points awarded",
                amount, rule.MinAmount, rule.Name);
            return;
        }

        // ── Step 3: Calculate raw points ───────────────────────────────────
        var effectiveAmount = rule.MaxAmount.HasValue
            ? Math.Min(amount, rule.MaxAmount.Value)
            : amount;

        var rawPoints = (int)Math.Floor(effectiveAmount * rule.PointsPerRupee
                            * account.Tier.MultiplierFactor);

        // ── Step 4: Apply per-transaction cap ─────────────────────────────
        var pointsToAward = rule.MaxPointsPerTxn.HasValue
            ? Math.Min(rawPoints, rule.MaxPointsPerTxn.Value)
            : rawPoints;

        if (pointsToAward <= 0)
        {
            _logger.LogInformation(
                "Calculated 0 points for user {UserId} — skipping", userId);
            return;
        }

        // ── Step 5: Credit points and record transaction ───────────────────
        var balanceBefore = account.TotalPoints;

        account.TotalPoints    += pointsToAward;
        account.LifetimePoints += pointsToAward;

        await _loyaltyRepo.AddPointTransactionAsync(new PointTransaction
        {
            LoyaltyAccountId = account.Id,
            Type             = PointTransactionType.Earned,
            Points           = pointsToAward,
            BalanceBefore    = balanceBefore,
            BalanceAfter     = account.TotalPoints,
            Description      = $"Points earned from {transactionType} of ₹{amount:F2}",
            ReferenceId      = transactionId
        });

        // ── Step 6: Check for tier upgrade ────────────────────────────────
        var previousTierName = account.Tier.Name;
        var newTier          = await _loyaltyRepo.GetTierForPointsAsync(account.LifetimePoints);
        var tierUpgraded     = false;

        if (newTier is not null && newTier.Id != account.TierId)
        {
            account.TierId = newTier.Id;
            tierUpgraded   = true;
            _logger.LogInformation(
                "User {UserId} upgraded from {OldTier} to {NewTier}",
                userId, previousTierName, newTier.Name);
        }

        await _loyaltyRepo.UpdateAsync(account);

        // ── Step 7: Publish PointsAwardedEvent ────────────────────────────
        _eventPublisher.PublishPointsAwarded(new PointsAwardedEvent
        {
            UserId           = userId,
            LoyaltyAccountId = account.Id,
            PointsAwarded    = pointsToAward,
            TotalPoints      = account.TotalPoints,
            TierName         = newTier?.Name ?? previousTierName,
            TierUpgraded     = tierUpgraded,
            TransactionId    = transactionId
        });

        _logger.LogInformation(
            "Awarded {Points} points to user {UserId} | Total: {Total} | Tier: {Tier}",
            pointsToAward, userId, account.TotalPoints,
            tierUpgraded ? $"{previousTierName} → {newTier!.Name}" : previousTierName);
    }

    public async Task DeductPointsAsync(
        Guid userId, int points, Guid redemptionId, string itemName)
    {
        var account = await _loyaltyRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Loyalty account not found.");

        if (account.TotalPoints < points)
            throw new InvalidOperationException(
                $"Insufficient points. Available: {account.TotalPoints}, Required: {points}");

        var balanceBefore = account.TotalPoints;

        account.TotalPoints    -= points;
        account.RedeemedPoints += points;

        await _loyaltyRepo.AddPointTransactionAsync(new PointTransaction
        {
            LoyaltyAccountId = account.Id,
            Type             = PointTransactionType.Redeemed,
            Points           = -points,   // negative = spent
            BalanceBefore    = balanceBefore,
            BalanceAfter     = account.TotalPoints,
            Description      = $"Points redeemed for: {itemName}",
            ReferenceId      = redemptionId
        });

        await _loyaltyRepo.UpdateAsync(account);

        _logger.LogInformation(
            "Deducted {Points} points from user {UserId} for redemption {RedemptionId}",
            points, userId, redemptionId);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private async Task<LoyaltyAccountResponseDto> MapToResponseAsync(LoyaltyAccount account)
    {
        var nextTier = await _loyaltyRepo.GetNextTierAsync(account.LifetimePoints);

        return new LoyaltyAccountResponseDto
        {
            Id               = account.Id,
            UserId           = account.UserId,
            TotalPoints      = account.TotalPoints,
            LifetimePoints   = account.LifetimePoints,
            RedeemedPoints   = account.RedeemedPoints,
            TierName         = account.Tier.Name,
            TierBadgeColor   = account.Tier.BadgeColor,
            TierMultiplier   = account.Tier.MultiplierFactor,
            PointsToNextTier = nextTier is not null
                                   ? nextTier.MinPoints - account.LifetimePoints
                                   : 0,
            NextTierName     = nextTier?.Name ?? "Max tier reached",
            CreatedAt        = account.CreatedAt
        };
    }
}