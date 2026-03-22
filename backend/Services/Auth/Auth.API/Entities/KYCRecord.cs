using Auth.API.Enums;
//KYC record is first created it has not been reviewed by anyone yet Guid? resprests this
namespace Auth.API.Entities;

public class KYCRecord
{
    public Guid      Id             { get; set; } = Guid.NewGuid();
    public Guid      UserId         { get; set; }
    public string    DocumentType   { get; set; } = string.Empty; // Aadhaar, PAN, Passport
    public string    DocumentNumber { get; set; } = string.Empty;
    public string?   DocumentUrl    { get; set; }
    public KYCStatus Status         { get; set; } = KYCStatus.Pending;
    public string?   RejectionReason { get; set; }
    public Guid?     ReviewedBy     { get; set; }
    public DateTime  SubmittedAt    { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt     { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}