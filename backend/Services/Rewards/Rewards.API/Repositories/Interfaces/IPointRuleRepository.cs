using Rewards.API.Entities;
using Rewards.API.Enums;

namespace Rewards.API.Repositories.Interfaces;

public interface IPointRuleRepository
{
    Task<PointRule?>         GetActiveRuleAsync(TransactionTypeRef type);
    Task<List<PointRule>>    GetAllAsync();
    Task<PointRule>          CreateAsync(PointRule rule);
    Task<PointRule>          UpdateAsync(PointRule rule);
    Task                     DeactivateAsync(Guid ruleId);
}