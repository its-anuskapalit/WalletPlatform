using Catalog.API.Entities;
using Catalog.API.Enums;

namespace Catalog.API.Repositories.Interfaces;

public interface ICatalogRepository
{
    Task<List<CatalogItem>> GetAllActiveAsync(CatalogItemCategory? category);
    Task<CatalogItem?>      GetByIdAsync(Guid id);
    Task<CatalogItem>       CreateAsync(CatalogItem item);
    Task<CatalogItem>       UpdateAsync(CatalogItem item);
    Task                    DeactivateAsync(Guid id);
}