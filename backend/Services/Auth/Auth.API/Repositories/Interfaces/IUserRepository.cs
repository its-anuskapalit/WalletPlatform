using Auth.API.Entities;

namespace Auth.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?>          GetByIdAsync(Guid id);
    Task<User?>          GetByEmailAsync(string email);
    Task<User?>          GetByPhoneAsync(string phone);
    Task<bool>           EmailExistsAsync(string email); //During registration you only need to know if an email exists
    Task<bool>           PhoneExistsAsync(string phone);
    Task<User>           CreateAsync(User user);
    Task<User>           UpdateAsync(User user);
    Task<RefreshToken?>  GetRefreshTokenAsync(string token);
    Task                 AddRefreshTokenAsync(RefreshToken token);
    Task                 RevokeRefreshTokenAsync(string token);
    Task                 AddAuditLogAsync(AuditLog log);
}