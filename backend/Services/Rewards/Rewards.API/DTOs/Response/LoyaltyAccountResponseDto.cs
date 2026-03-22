namespace Rewards.API.DTOs.Response;

public class LoyaltyAccountResponseDto
{
    public Guid    Id             { get; set; }
    public Guid    UserId         { get; set; }
    public int     TotalPoints    { get; set; }
    public int     LifetimePoints { get; set; }
    public int     RedeemedPoints { get; set; }
    public string  TierName       { get; set; } = string.Empty;
    public string  TierBadgeColor { get; set; } = string.Empty;
    public decimal TierMultiplier { get; set; }
    public int     PointsToNextTier { get; set; }
    public string  NextTierName   { get; set; } = string.Empty;
    public DateTime CreatedAt     { get; set; }
}