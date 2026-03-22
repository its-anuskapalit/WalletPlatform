namespace Rewards.API.DTOs.Request;

public class CreateRewardTierRequestDto
{
    public string  Name             { get; set; } = string.Empty;
    public int     MinPoints        { get; set; }
    public int     MaxPoints        { get; set; }
    public decimal MultiplierFactor { get; set; }
    public string  BadgeColor       { get; set; } = string.Empty;
    public int     DisplayOrder     { get; set; }
}