using Microsoft.EntityFrameworkCore;
using Rewards.API.Data;
using Rewards.API.Entities;
using Rewards.API.Repositories.Interfaces;

namespace Rewards.API.Repositories.Implementations;

public class LoyaltyRepository : ILoyaltyRepository
{
    private readonly RewardsDbContext _context;

    public LoyaltyRepository(RewardsDbContext context)
    {
        _context = context;
    }

    public async Task<LoyaltyAccount?> GetByUserIdAsync(Guid userId) =>
        await _context.LoyaltyAccounts
            .Include(a => a.Tier)
            .FirstOrDefaultAsync(a => a.UserId == userId);

    public async Task<LoyaltyAccount?> GetByIdAsync(Guid id) =>
        await _context.LoyaltyAccounts
            .Include(a => a.Tier)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<LoyaltyAccount> CreateAsync(LoyaltyAccount account)
    {
        _context.LoyaltyAccounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<LoyaltyAccount> UpdateAsync(LoyaltyAccount account)
    {
        account.UpdatedAt = DateTime.UtcNow;
        _context.LoyaltyAccounts.Update(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<PointTransaction> AddPointTransactionAsync(PointTransaction transaction)
    {
        _context.PointTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<List<PointTransaction>> GetPointHistoryAsync(
        Guid loyaltyAccountId, int page, int pageSize) =>
        await _context.PointTransactions
            .Where(t => t.LoyaltyAccountId == loyaltyAccountId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<RewardTier?> GetTierForPointsAsync(int points) =>
        await _context.RewardTiers
            .Where(t => t.IsActive &&
                        t.MinPoints <= points &&
                        (t.MaxPoints == -1 || t.MaxPoints >= points))
            .OrderByDescending(t => t.MinPoints)
            .FirstOrDefaultAsync();

    public async Task<RewardTier?> GetNextTierAsync(int currentPoints) =>
        await _context.RewardTiers
            .Where(t => t.IsActive && t.MinPoints > currentPoints)
            .OrderBy(t => t.MinPoints)
            .FirstOrDefaultAsync();

    public async Task<List<RewardTier>> GetAllTiersAsync() =>
        await _context.RewardTiers
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();
}