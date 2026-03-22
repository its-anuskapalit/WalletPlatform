namespace Auth.API.DTOs.Response;

public class KYCResponseDto
{
    public Guid      Id             { get; set; }
    public string    DocumentType   { get; set; } = string.Empty;
    public string    Status         { get; set; } = string.Empty;
    public string?   RejectionReason { get; set; }
    public DateTime  SubmittedAt    { get; set; }
    public DateTime? ReviewedAt     { get; set; }
}