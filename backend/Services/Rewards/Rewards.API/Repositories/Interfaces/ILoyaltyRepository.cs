using Rewards.API.Entities;

namespace Rewards.API.Repositories.Interfaces;

public interface ILoyaltyRepository
{
    Task<LoyaltyAccount?>          GetByUserIdAsync(Guid userId);
    Task<LoyaltyAccount?>          GetByIdAsync(Guid id);
    Task<LoyaltyAccount>           CreateAsync(LoyaltyAccount account);
    Task<LoyaltyAccount>           UpdateAsync(LoyaltyAccount account);
    Task<PointTransaction>         AddPointTransactionAsync(PointTransaction transaction);
    Task<List<PointTransaction>>   GetPointHistoryAsync(Guid loyaltyAccountId, int page, int pageSize);
    Task<RewardTier?>              GetTierForPointsAsync(int points);
    Task<RewardTier?>              GetNextTierAsync(int currentPoints);
    Task<List<RewardTier>>         GetAllTiersAsync();
}