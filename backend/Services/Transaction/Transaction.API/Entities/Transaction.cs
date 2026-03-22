using Transaction.API.Enums;

namespace Transaction.API.Entities;

public class Transaction
{
    public Guid              Id              { get; set; } = Guid.NewGuid();
    public Guid              SenderId        { get; set; }
    public Guid              RecipientId     { get; set; }
    public decimal           Amount          { get; set; }
    public string            Currency        { get; set; } = "INR";
    public TransactionType   Type            { get; set; }
    public TransactionStatus Status          { get; set; } = TransactionStatus.Pending;
    public string            Description     { get; set; } = string.Empty;
    public string            IdempotencyKey  { get; set; } = string.Empty;
    public string?           FailureReason   { get; set; }
    public string?           ReferenceId     { get; set; } // external ref (merchant order id etc.)
    public DateTime          CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime?         CompletedAt     { get; set; }
    public DateTime?         ReversedAt      { get; set; }

    // Navigation
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}