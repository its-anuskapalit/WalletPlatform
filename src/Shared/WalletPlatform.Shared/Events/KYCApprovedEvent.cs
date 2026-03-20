namespace WalletPlatform.Shared.Events;

public class KYCApprovedEvent
{
    public Guid     UserId     { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}