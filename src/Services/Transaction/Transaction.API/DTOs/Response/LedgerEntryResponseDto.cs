namespace Transaction.API.DTOs.Response;

public class LedgerEntryResponseDto
{
    public Guid     Id            { get; set; }
    public Guid     AccountId     { get; set; }
    public string   EntryType     { get; set; } = string.Empty;
    public decimal  Amount        { get; set; }
    public decimal  BalanceBefore { get; set; }
    public decimal  BalanceAfter  { get; set; }
    public string   Description   { get; set; } = string.Empty;
    public DateTime CreatedAt     { get; set; }
}