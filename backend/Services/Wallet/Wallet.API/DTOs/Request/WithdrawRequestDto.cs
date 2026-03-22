namespace Wallet.API.DTOs.Request;
public class WithdrawRequestDto
{
    public decimal Amount          { get; set; }
    public string  PaymentMethodId { get; set; } = string.Empty;
    public string  Description     { get; set; } = string.Empty;
}