namespace WalletPlatform.Shared.Events;
public class PointsAwardedEvent
{
    public Guid     UserId          { get; set; }
    public Guid     LoyaltyAccountId { get; set; }
    public int      PointsAwarded   { get; set; }
    public int      TotalPoints     { get; set; }
    public string   TierName        { get; set; } = string.Empty;
    public bool     TierUpgraded    { get; set; } = false; //Notification.API for template choose
    public Guid     TransactionId   { get; set; }
    public DateTime OccurredAt      { get; set; } = DateTime.UtcNow;
}