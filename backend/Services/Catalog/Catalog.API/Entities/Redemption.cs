using Catalog.API.Enums;

namespace Catalog.API.Entities;

public class Redemption
{
    public Guid             Id             { get; set; } = Guid.NewGuid();
    public Guid             UserId         { get; set; }  // denormalized ref
    public Guid             CatalogItemId  { get; set; }
    public int              PointsSpent    { get; set; }
    public RedemptionStatus Status         { get; set; } = RedemptionStatus.Pending;
    public string?          VoucherCode    { get; set; }  // generated on completion
    public string?          FailureReason  { get; set; }
    public DateTime         CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime?        CompletedAt    { get; set; }

    // Navigation
    public CatalogItem CatalogItem { get; set; } = null!;
}