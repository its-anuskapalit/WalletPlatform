using Transaction.API.DTOs.Request;
using Transaction.API.DTOs.Response;

namespace Transaction.API.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponseDto> InitiatePaymentAsync(
        Guid senderId,
        string idempotencyKey,
        InitiatePaymentRequestDto dto);

    Task<TransactionResponseDto>       GetTransactionAsync(Guid transactionId);
    Task<List<TransactionResponseDto>> GetUserTransactionsAsync(Guid userId, int page, int pageSize);
    Task                               HandleTransactionFailedAsync(Guid transactionId, string reason);
}