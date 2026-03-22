using Rewards.API.DTOs.Response;

namespace Rewards.API.Services.Interfaces;

public interface IRewardsService
{
    Task<LoyaltyAccountResponseDto>        GetAccountAsync(Guid userId);
    Task<List<PointTransactionResponseDto>> GetHistoryAsync(Guid userId, int page, int pageSize);
    Task<List<RewardTierResponseDto>>      GetTiersAsync();
    Task                                   CreateAccountAsync(Guid userId);
    Task                                   AwardPointsAsync(Guid userId, decimal amount,
                                               string transactionType, Guid transactionId);
    Task                                   DeductPointsAsync(Guid userId, int points,
                                               Guid redemptionId, string itemName);
}