using Rewards.API.DTOs.Request;
using Rewards.API.DTOs.Response;
using Rewards.API.Entities;
using Rewards.API.Enums;
using Rewards.API.Repositories.Interfaces;
using Rewards.API.Services.Interfaces;

namespace Rewards.API.Services.Implementations;

public class PointRuleService : IPointRuleService
{
    private readonly IPointRuleRepository _ruleRepo;

    public PointRuleService(IPointRuleRepository ruleRepo)
    {
        _ruleRepo = ruleRepo;
    }

    public async Task<List<PointRuleResponseDto>> GetAllAsync()
    {
        var rules = await _ruleRepo.GetAllAsync();
        return rules.Select(MapToResponse).ToList();
    }

    public async Task<PointRuleResponseDto> CreateAsync(CreatePointRuleRequestDto dto)
    {
        if (!Enum.TryParse<TransactionTypeRef>(dto.TransactionType, out var typeRef))
            throw new InvalidOperationException(
                $"Invalid transaction type: {dto.TransactionType}");

        var rule = new PointRule
        {
            Name            = dto.Name,
            TransactionType = typeRef,
            PointsPerRupee  = dto.PointsPerRupee,
            MinAmount       = dto.MinAmount,
            MaxAmount       = dto.MaxAmount,
            MaxPointsPerTxn = dto.MaxPointsPerTxn,
            IsActive        = true
        };

        var created = await _ruleRepo.CreateAsync(rule);
        return MapToResponse(created);
    }

    public async Task DeactivateAsync(Guid ruleId) =>
        await _ruleRepo.DeactivateAsync(ruleId);

    private static PointRuleResponseDto MapToResponse(PointRule r) => new()
    {
        Id              = r.Id,
        Name            = r.Name,
        TransactionType = r.TransactionType.ToString(),
        PointsPerRupee  = r.PointsPerRupee,
        MinAmount       = r.MinAmount,
        MaxAmount       = r.MaxAmount,
        MaxPointsPerTxn = r.MaxPointsPerTxn,
        IsActive        = r.IsActive
    };
}