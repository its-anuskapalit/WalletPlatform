using Catalog.API.Entities;

namespace Catalog.API.Repositories.Interfaces;

public interface IRedemptionRepository
{
    Task<Redemption>       CreateAsync(Redemption redemption);
    Task<Redemption?>      GetByIdAsync(Guid id);
    Task<Redemption>       UpdateAsync(Redemption redemption);
    Task<List<Redemption>> GetByUserIdAsync(Guid userId, int page, int pageSize);
}