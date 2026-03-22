namespace Transaction.API.DTOs.Request;

public class InitiatePaymentRequestDto
{
    public Guid    RecipientId    { get; set; }
    public decimal Amount         { get; set; }
    public string  Description    { get; set; } = string.Empty;
    public string? ReferenceId    { get; set; }
}