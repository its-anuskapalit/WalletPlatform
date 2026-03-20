namespace Catalog.API.DTOs.Response;

public class RedemptionResponseDto
{
    public Guid      Id            { get; set; }
    public Guid      UserId        { get; set; }
    public string    ItemName      { get; set; } = string.Empty;
    public int       PointsSpent   { get; set; }
    public string    Status        { get; set; } = string.Empty;
    public string?   VoucherCode   { get; set; }
    public string?   FailureReason { get; set; }
    public DateTime  CreatedAt     { get; set; }
    public DateTime? CompletedAt   { get; set; }
}