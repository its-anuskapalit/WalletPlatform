//double, float: binary where 0.1+0.2=0.30004 in temp coor its fine by in money nope thus decimal
//IdempotencyKey : Transaction service uses this key to detect duplicate processin

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