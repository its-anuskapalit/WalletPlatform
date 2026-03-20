using Catalog.API.DTOs.Request;
using Catalog.API.DTOs.Response;

namespace Catalog.API.Services.Interfaces;

public interface IRedemptionService
{
    Task<RedemptionResponseDto>       RedeemAsync(Guid userId, RedeemItemRequestDto dto);
    Task<List<RedemptionResponseDto>> GetUserRedemptionsAsync(Guid userId, int page, int pageSize);
    Task<RedemptionResponseDto>       GetByIdAsync(Guid redemptionId);
    Task                              CompleteRedemptionAsync(Guid redemptionId);
    Task                              FailRedemptionAsync(Guid redemptionId, string reason);
}