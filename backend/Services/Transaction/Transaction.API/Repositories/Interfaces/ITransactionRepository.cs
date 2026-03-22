using Transaction.API.Entities;
using Transaction.API.Enums;

namespace Transaction.API.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction.API.Entities.Transaction?>  GetByIdAsync(Guid id);
    Task<Transaction.API.Entities.Transaction?>  GetByIdempotencyKeyAsync(string key, Guid userId);
    Task<Transaction.API.Entities.Transaction>   CreateAsync(Transaction.API.Entities.Transaction transaction);
    Task<Transaction.API.Entities.Transaction>   UpdateStatusAsync(Guid id, TransactionStatus status, string? failureReason = null);
    Task<List<Transaction.API.Entities.Transaction>> GetByUserIdAsync(Guid userId, int page, int pageSize);
    Task<IdempotencyRecord?>  GetIdempotencyRecordAsync(string key, Guid userId);
    Task                      SaveIdempotencyRecordAsync(IdempotencyRecord record);
}