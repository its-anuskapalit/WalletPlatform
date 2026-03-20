namespace WalletPlatform.Shared.Events;

public class PaymentInitiatedEvent
{
    public Guid TransactionId  { get; set; }
    public Guid SenderId       { get; set; }
    public Guid RecipientId    { get; set; }
    public decimal Amount      { get; set; }
    public string Currency     { get; set; } = "INR";
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}