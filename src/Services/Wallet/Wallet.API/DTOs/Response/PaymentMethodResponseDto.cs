namespace Wallet.API.DTOs.Response;

public class PaymentMethodResponseDto
{
    public Guid   Id          { get; set; }
    public string Type        { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Last4Digits { get; set; }
    public string? BankName   { get; set; }
    public string? UpiId      { get; set; }
    public bool   IsDefault   { get; set; }
}