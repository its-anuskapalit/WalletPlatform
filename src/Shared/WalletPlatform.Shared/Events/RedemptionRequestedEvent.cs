namespace WalletPlatform.Shared.Events;

public class RedemptionRequestedEvent
{
    public Guid     UserId       { get; set; }
    public Guid     ItemId       { get; set; }
    public string   ItemName     { get; set; } = string.Empty;
    public int      PointsCost   { get; set; }
    public DateTime OccurredAt   { get; set; } = DateTime.UtcNow;
}