using Wallet.API.Enums;

namespace Wallet.API.Entities;
public class Wallet
{
    public Guid         Id             { get; set; } = Guid.NewGuid();
    public Guid         UserId         { get; set; }  // denormalized ref to Auth service
    public string       WalletNumber   { get; set; } = string.Empty;
    public decimal      Balance        { get; set; } = 0.00m;
    public decimal      FrozenAmount   { get; set; } = 0.00m; // reserved/held amount
    public string       Currency       { get; set; } = "INR";
    public WalletStatus Status         { get; set; } = WalletStatus.Pending;
    public DateTime     CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime?    UpdatedAt      { get; set; }

    // Navigation
    public ICollection<PaymentMethod>  PaymentMethods { get; set; } = new List<PaymentMethod>();
    public ICollection<WalletFreezeLog> FreezeLogs    { get; set; } = new List<WalletFreezeLog>();

    // Computed — never stored in DB
    public decimal AvailableBalance => Balance - FrozenAmount;
}