using Catalog.API.Data;
using Catalog.API.Entities;
using Catalog.API.Enums;
using Catalog.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Repositories.Implementations;

public class CatalogRepository : ICatalogRepository
{
    private readonly CatalogDbContext _context;

    public CatalogRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<List<CatalogItem>> GetAllActiveAsync(CatalogItemCategory? category)
    {
        var query = _context.CatalogItems
            .Where(c => c.IsActive &&
                        c.ValidFrom <= DateTime.UtcNow &&
                        (c.ValidUntil == null || c.ValidUntil > DateTime.UtcNow));

        if (category.HasValue)
            query = query.Where(c => c.Category == category.Value);

        return await query
            .OrderBy(c => c.PointsCost)
            .ToListAsync();
    }

    public async Task<CatalogItem?> GetByIdAsync(Guid id) =>
        await _context.CatalogItems.FindAsync(id);

    public async Task<CatalogItem> CreateAsync(CatalogItem item)
    {
        _context.CatalogItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<CatalogItem> UpdateAsync(CatalogItem item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.CatalogItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeactivateAsync(Guid id)
    {
        var item = await _context.CatalogItems.FindAsync(id);
        if (item is not null)
        {
            item.IsActive  = false;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}