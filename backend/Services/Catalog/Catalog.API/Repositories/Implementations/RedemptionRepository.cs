using Catalog.API.Data;
using Catalog.API.Entities;
using Catalog.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Repositories.Implementations;

public class RedemptionRepository : IRedemptionRepository
{
    private readonly CatalogDbContext _context;

    public RedemptionRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Redemption> CreateAsync(Redemption redemption)
    {
        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();
        return redemption;
    }

    public async Task<Redemption?> GetByIdAsync(Guid id) =>
        await _context.Redemptions
            .Include(r => r.CatalogItem)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Redemption> UpdateAsync(Redemption redemption)
    {
        _context.Redemptions.Update(redemption);
        await _context.SaveChangesAsync();
        return redemption;
    }

    public async Task<List<Redemption>> GetByUserIdAsync(
        Guid userId, int page, int pageSize) =>
        await _context.Redemptions
            .Include(r => r.CatalogItem)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
}