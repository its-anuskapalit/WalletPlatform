namespace Auth.API.DTOs.Request;

public class KYCSubmitRequestDto
{
    public string DocumentType   { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string? DocumentUrl   { get; set; }
}