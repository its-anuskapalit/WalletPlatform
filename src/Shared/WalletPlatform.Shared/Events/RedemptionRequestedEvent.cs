// Rewards.API needs to write a PointTransaction with description "Points redeemed for: Amazon ₹100 Voucher". If only ItemId were included, Rewards.API would need to call Catalog.API to look up the item name — a synchronous cross-service call that creates coupling. Including ItemName as a string keeps the event self-contained
namespace WalletPlatform.Shared.Events;
public class RedemptionRequestedEvent
{
    public Guid     UserId       { get; set; }
    public Guid     ItemId       { get; set; }
    public string   ItemName     { get; set; } = string.Empty;
    public int      PointsCost   { get; set; }
    public DateTime OccurredAt   { get; set; } = DateTime.UtcNow;
}