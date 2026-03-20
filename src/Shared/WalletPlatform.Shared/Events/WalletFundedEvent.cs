namespace WalletPlatform.Shared.Events;

public class WalletFundedEvent
{
    public Guid     UserId     { get; set; }
    public Guid     WalletId   { get; set; }
    public decimal  Amount     { get; set; }
    public string   Currency   { get; set; } = "INR";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}