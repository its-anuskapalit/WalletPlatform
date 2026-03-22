using Transaction.API.Entities;

namespace Transaction.API.Repositories.Interfaces;

public interface ILedgerRepository
{
    Task<List<LedgerEntry>> GetByAccountIdAsync(Guid accountId, int page, int pageSize);
    Task<List<LedgerEntry>> GetByTransactionIdAsync(Guid transactionId);
    Task<LedgerEntry>       CreateAsync(LedgerEntry entry);
    Task                    CreateManyAsync(IEnumerable<LedgerEntry> entries);
    Task<decimal>           GetAccountBalanceAsync(Guid accountId);
}