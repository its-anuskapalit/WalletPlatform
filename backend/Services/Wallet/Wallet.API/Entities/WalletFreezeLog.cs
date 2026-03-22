namespace Wallet.API.Entities;

public class WalletFreezeLog
{
    public Guid     Id          { get; set; } = Guid.NewGuid();
    public Guid     WalletId    { get; set; }
    public Guid     ActionBy    { get; set; }  // Admin userId
    public string   Action      { get; set; } = string.Empty; // "FREEZE" | "UNFREEZE"
    public string   Reason      { get; set; } = string.Empty;
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
    // Navigation
    public Wallet Wallet { get; set; } = null!;
}