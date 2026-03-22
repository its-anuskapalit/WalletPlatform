namespace Rewards.API.DTOs.Request;

public class CreatePointRuleRequestDto
{
    public string  Name            { get; set; } = string.Empty;
    public string  TransactionType { get; set; } = string.Empty;
    public decimal PointsPerRupee  { get; set; }
    public decimal? MinAmount      { get; set; }
    public decimal? MaxAmount      { get; set; }
    public int?    MaxPointsPerTxn { get; set; }
}