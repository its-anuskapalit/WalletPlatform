namespace Rewards.API.Entities;

public class LoyaltyAccount
{
    public Guid     Id              { get; set; } = Guid.NewGuid();
    public Guid     UserId          { get; set; }  // denormalized ref to Auth service
    public Guid     TierId          { get; set; }
    public int      TotalPoints     { get; set; } = 0;  // current spendable points
    public int      LifetimePoints  { get; set; } = 0;  // never decreases — used for tier calc
    public int      RedeemedPoints  { get; set; } = 0;
    public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt      { get; set; }

    // Navigation
    public RewardTier                    Tier         { get; set; } = null!;
    public ICollection<PointTransaction> Transactions { get; set; } = new List<PointTransaction>();
}