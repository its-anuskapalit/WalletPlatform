namespace Rewards.API.Entities;

public class RewardTier
{
    public Guid    Id               { get; set; } = Guid.NewGuid();
    public string  Name             { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum
    public int     MinPoints        { get; set; }  // Lower bound (inclusive)
    public int     MaxPoints        { get; set; }  // Upper bound (exclusive, -1 = unlimited)
    public decimal MultiplierFactor { get; set; } = 1.0m; // Points multiplier for this tier
    public string  BadgeColor       { get; set; } = string.Empty;
    public int     DisplayOrder     { get; set; }
    public bool    IsActive         { get; set; } = true;
    public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<LoyaltyAccount> Accounts { get; set; } = new List<LoyaltyAccount>();
}