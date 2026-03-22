using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rewards.API.DTOs.Request;
using Rewards.API.Services.Interfaces;
using WalletPlatform.Shared.Models;

namespace Rewards.API.Controllers;

[ApiController]
[Route("api/admin/rewards")]
[Authorize(Roles = "Admin")]
public class AdminRewardsController : ControllerBase
{
    private readonly IPointRuleService _pointRuleService;

    public AdminRewardsController(IPointRuleService pointRuleService)
    {
        _pointRuleService = pointRuleService;
    }

    /// <summary>Get all point rules</summary>
    [HttpGet("rules")]
    public async Task<IActionResult> GetRules()
    {
        var result = await _pointRuleService.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Create a new point rule</summary>
    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule([FromBody] CreatePointRuleRequestDto dto)
    {
        var result = await _pointRuleService.CreateAsync(dto);
        return Ok(ApiResponse<object>.Ok(result, "Point rule created."));
    }

    /// <summary>Deactivate a point rule</summary>
    [HttpDelete("rules/{ruleId}")]
    public async Task<IActionResult> DeactivateRule(Guid ruleId)
    {
        await _pointRuleService.DeactivateAsync(ruleId);
        return Ok(ApiResponse<object>.Ok(null, "Point rule deactivated."));
    }
}