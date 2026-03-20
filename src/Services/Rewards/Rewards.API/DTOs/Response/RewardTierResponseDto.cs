namespace Rewards.API.DTOs.Response;

public class RewardTierResponseDto
{
    public Guid    Id               { get; set; }
    public string  Name             { get; set; } = string.Empty;
    public int     MinPoints        { get; set; }
    public int     MaxPoints        { get; set; }
    public decimal MultiplierFactor { get; set; }
    public string  BadgeColor       { get; set; } = string.Empty;
    public int     DisplayOrder     { get; set; }
}