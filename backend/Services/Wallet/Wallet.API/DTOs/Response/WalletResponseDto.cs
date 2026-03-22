namespace Wallet.API.DTOs.Response;
public class WalletResponseDto
{
    public Guid     Id               { get; set; }
    public Guid     UserId           { get; set; }
    public string   WalletNumber     { get; set; } = string.Empty;
    public decimal  Balance          { get; set; }
    public decimal  FrozenAmount     { get; set; }
    public decimal  AvailableBalance { get; set; }
    public string   Currency         { get; set; } = string.Empty;
    public string   Status           { get; set; } = string.Empty;
    public DateTime CreatedAt        { get; set; }
    public List<PaymentMethodResponseDto> PaymentMethods { get; set; } = new();
}