using Auth.API.DTOs.Request;
using Auth.API.DTOs.Response;

namespace Auth.API.Services.Interfaces;

public interface IKYCService
{
    Task<KYCResponseDto>       SubmitAsync(Guid userId, KYCSubmitRequestDto dto);
    Task<KYCResponseDto>       GetStatusAsync(Guid userId);
    Task<KYCResponseDto>       ApproveAsync(Guid kycId, Guid adminId);
    Task<KYCResponseDto>       RejectAsync(Guid kycId, Guid adminId, string reason);
    Task<List<KYCResponseDto>> GetPendingAsync();
}