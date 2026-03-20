using Rewards.API.Enums;

namespace Rewards.API.Entities;

public class PointTransaction
{
    public Guid                Id               { get; set; } = Guid.NewGuid();
    public Guid                LoyaltyAccountId { get; set; }
    public PointTransactionType Type            { get; set; }
    public int                 Points           { get; set; }  // positive = earned, negative = redeemed
    public int                 BalanceBefore    { get; set; }  // snapshot for audit
    public int                 BalanceAfter     { get; set; }  // snapshot for audit
    public string              Description      { get; set; } = string.Empty;
    public Guid?               ReferenceId      { get; set; }  // TransactionId or RedemptionId
    public DateTime            CreatedAt        { get; set; } = DateTime.UtcNow;

    // Navigation
    public LoyaltyAccount LoyaltyAccount { get; set; } = null!;
}