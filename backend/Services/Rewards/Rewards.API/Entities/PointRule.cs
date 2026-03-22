using Rewards.API.Enums;

namespace Rewards.API.Entities;

public class PointRule
{
    public Guid               Id              { get; set; } = Guid.NewGuid();
    public string             Name            { get; set; } = string.Empty;
    public TransactionTypeRef TransactionType { get; set; }
    public decimal            PointsPerRupee  { get; set; } // e.g. 0.1 = 1 point per ₹10
    public decimal?           MinAmount       { get; set; } // only apply if amount >= this
    public decimal?           MaxAmount       { get; set; } // cap the points calculation at this amount
    public int?               MaxPointsPerTxn { get; set; } // absolute cap per transaction
    public bool               IsActive        { get; set; } = true;
    public DateTime           CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime?          UpdatedAt       { get; set; }
}