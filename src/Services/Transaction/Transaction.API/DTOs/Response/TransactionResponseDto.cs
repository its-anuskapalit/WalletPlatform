namespace Transaction.API.DTOs.Response;

public class TransactionResponseDto
{
    public Guid     Id             { get; set; }
    public Guid     SenderId       { get; set; }
    public Guid     RecipientId    { get; set; }
    public decimal  Amount         { get; set; }
    public string   Currency       { get; set; } = string.Empty;
    public string   Type           { get; set; } = string.Empty;
    public string   Status         { get; set; } = string.Empty;
    public string   Description    { get; set; } = string.Empty;
    public string?  FailureReason  { get; set; }
    public DateTime CreatedAt      { get; set; }
    public DateTime? CompletedAt   { get; set; }

    public List<LedgerEntryResponseDto> LedgerEntries { get; set; } = new();
}