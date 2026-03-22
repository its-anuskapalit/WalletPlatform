
namespace WalletPlatform.Shared.Events;
public class WalletFundedEvent
{
    public Guid     UserId     { get; set; } //notification.api
    public Guid     WalletId   { get; set; } //transaction.api
    public decimal  Amount     { get; set; }
    public string   Currency   { get; set; } = "INR";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}