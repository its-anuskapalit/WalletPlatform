using Transaction.API.Enums;

namespace Transaction.API.Entities;

public class LedgerEntry
{
    public Guid             Id            { get; set; } = Guid.NewGuid();
    public Guid             TransactionId { get; set; }
    public Guid             AccountId     { get; set; }  // UserId acting as account
    public LedgerEntryType  EntryType     { get; set; }  // Debit or Credit
    public decimal          Amount        { get; set; }
    public string           Currency      { get; set; } = "INR";
    public decimal          BalanceBefore { get; set; }  // Snapshot for audit
    public decimal          BalanceAfter  { get; set; }  // Snapshot for audit
    public string           Description   { get; set; } = string.Empty;
    public DateTime         CreatedAt     { get; set; } = DateTime.UtcNow;

    // Navigation
    public Transaction Transaction { get; set; } = null!;
}