using Auth.API.Entities;
using Auth.API.Enums;

namespace Auth.API.Repositories.Interfaces;

public interface IKYCRepository
{
    Task<KYCRecord?>  GetByUserIdAsync(Guid userId);
    Task<KYCRecord>   CreateAsync(KYCRecord record);
    Task<KYCRecord>   UpdateAsync(KYCRecord record);
    Task<List<KYCRecord>> GetPendingAsync();
    Task<KYCRecord?>  GetByIdAsync(Guid id);
}