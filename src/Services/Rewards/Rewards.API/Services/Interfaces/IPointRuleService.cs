using Rewards.API.DTOs.Request;
using Rewards.API.DTOs.Response;

namespace Rewards.API.Services.Interfaces;

public interface IPointRuleService
{
    Task<List<PointRuleResponseDto>> GetAllAsync();
    Task<PointRuleResponseDto>       CreateAsync(CreatePointRuleRequestDto dto);
    Task                             DeactivateAsync(Guid ruleId);
}