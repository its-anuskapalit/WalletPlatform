using Transaction.API.DTOs.Response;
using Transaction.API.Repositories.Interfaces;
using Transaction.API.Services.Interfaces;

namespace Transaction.API.Services.Implementations;

public class LedgerService : ILedgerService
{
    private readonly ILedgerRepository _ledgerRepo;

    public LedgerService(ILedgerRepository ledgerRepo)
    {
        _ledgerRepo = ledgerRepo;
    }

    public async Task<List<LedgerEntryResponseDto>> GetAccountLedgerAsync(
        Guid accountId, int page, int pageSize)
    {
        var entries = await _ledgerRepo.GetByAccountIdAsync(accountId, page, pageSize);
        return entries.Select(l => new LedgerEntryResponseDto
        {
            Id            = l.Id,
            AccountId     = l.AccountId,
            EntryType     = l.EntryType.ToString(),
            Amount        = l.Amount,
            BalanceBefore = l.BalanceBefore,
            BalanceAfter  = l.BalanceAfter,
            Description   = l.Description,
            CreatedAt     = l.CreatedAt
        }).ToList();
    }

    public async Task<decimal> GetDerivedBalanceAsync(Guid accountId) =>
        await _ledgerRepo.GetAccountBalanceAsync(accountId);
}