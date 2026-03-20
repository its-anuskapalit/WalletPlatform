namespace Transaction.API.Entities;

public class IdempotencyRecord
{
    public Guid     Id             { get; set; } = Guid.NewGuid();
    public string   IdempotencyKey { get; set; } = string.Empty;
    public Guid     UserId         { get; set; }
    public Guid     TransactionId  { get; set; }
    public string   ResponseJson   { get; set; } = string.Empty; // cached response
    public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt      { get; set; } = DateTime.UtcNow.AddHours(24);
}