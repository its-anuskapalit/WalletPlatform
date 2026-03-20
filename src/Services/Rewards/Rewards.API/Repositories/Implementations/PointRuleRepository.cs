using Microsoft.EntityFrameworkCore;
using Rewards.API.Data;
using Rewards.API.Entities;
using Rewards.API.Enums;
using Rewards.API.Repositories.Interfaces;

namespace Rewards.API.Repositories.Implementations;

public class PointRuleRepository : IPointRuleRepository
{
    private readonly RewardsDbContext _context;

    public PointRuleRepository(RewardsDbContext context)
    {
        _context = context;
    }

    public async Task<PointRule?> GetActiveRuleAsync(TransactionTypeRef type) =>
        await _context.PointRules
            .FirstOrDefaultAsync(r => r.TransactionType == type && r.IsActive);

    public async Task<List<PointRule>> GetAllAsync() =>
        await _context.PointRules
            .OrderBy(r => r.TransactionType)
            .ToListAsync();

    public async Task<PointRule> CreateAsync(PointRule rule)
    {
        _context.PointRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<PointRule> UpdateAsync(PointRule rule)
    {
        rule.UpdatedAt = DateTime.UtcNow;
        _context.PointRules.Update(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task DeactivateAsync(Guid ruleId)
    {
        var rule = await _context.PointRules.FindAsync(ruleId);
        if (rule is not null)
        {
            rule.IsActive  = false;
            rule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}