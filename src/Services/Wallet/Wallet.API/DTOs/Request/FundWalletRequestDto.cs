namespace Wallet.API.DTOs.Request;

public class FundWalletRequestDto
{
    public decimal Amount          { get; set; }
    public string  PaymentMethodId { get; set; } = string.Empty;
    public string  Description     { get; set; } = string.Empty;
}