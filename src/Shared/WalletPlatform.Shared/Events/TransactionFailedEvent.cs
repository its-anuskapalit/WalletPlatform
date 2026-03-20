namespace WalletPlatform.Shared.Events;

public class TransactionFailedEvent
{
    public Guid     TransactionId { get; set; }
    public Guid     SenderId      { get; set; }
    public decimal  Amount        { get; set; }
    public string   Reason        { get; set; } = string.Empty;
    public DateTime OccurredAt    { get; set; } = DateTime.UtcNow;
}