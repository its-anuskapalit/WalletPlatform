namespace WalletPlatform.Shared.Events;

public class TransactionCompletedEvent
{
    public Guid TransactionId { get; set; }
    public Guid UserId        { get; set; }
    public decimal Amount     { get; set; }
    public string Currency    { get; set; } = "INR";
    public string Description { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}