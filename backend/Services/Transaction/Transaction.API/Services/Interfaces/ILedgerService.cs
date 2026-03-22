using Transaction.API.DTOs.Response;

namespace Transaction.API.Services.Interfaces;

public interface ILedgerService
{
    Task<List<LedgerEntryResponseDto>> GetAccountLedgerAsync(Guid accountId, int page, int pageSize);
    Task<decimal>                      GetDerivedBalanceAsync(Guid accountId);
}