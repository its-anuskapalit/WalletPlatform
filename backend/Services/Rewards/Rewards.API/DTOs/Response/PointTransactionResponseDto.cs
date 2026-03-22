namespace Rewards.API.DTOs.Response;

public class PointTransactionResponseDto
{
    public Guid     Id            { get; set; }
    public string   Type          { get; set; } = string.Empty;
    public int      Points        { get; set; }
    public int      BalanceBefore { get; set; }
    public int      BalanceAfter  { get; set; }
    public string   Description   { get; set; } = string.Empty;
    public Guid?    ReferenceId   { get; set; }
    public DateTime CreatedAt     { get; set; }
}