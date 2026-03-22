using Wallet.API.Enums;

namespace Wallet.API.Entities;

public class PaymentMethod
{
    public Guid              Id            { get; set; } = Guid.NewGuid();
    public Guid              WalletId      { get; set; }
    public PaymentMethodType Type          { get; set; }
    public string            DisplayName   { get; set; } = string.Empty;

    // Tokenized — never store raw card numbers
    public string            Token         { get; set; } = string.Empty;
    public string?           Last4Digits   { get; set; }
    public string?           BankName      { get; set; }
    public string?           UpiId         { get; set; }
    public bool              IsDefault     { get; set; } = false;
    public bool              IsActive      { get; set; } = true;
    public DateTime          CreatedAt     { get; set; } = DateTime.UtcNow;

    // Navigation
    public Wallet Wallet { get; set; } = null!;
}