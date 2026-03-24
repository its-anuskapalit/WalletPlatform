using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rewards.API.DTOs.Request;
using Rewards.API.Services.Interfaces;
using WalletPlatform.Shared.Models;

namespace Rewards.API.Controllers;

// Marks this class as an API controller (auto validation, binding, etc.)
[ApiController]

// Base route: api/admin/rewards (clearly scoped under admin)
[Route("api/admin/rewards")]

// Restricts all endpoints to Admin role only (role-based authorization)
[Authorize(Roles = "Admin")]
public class AdminRewardsController : ControllerBase
{
    // Service dependency for managing reward point rules
    private readonly IPointRuleService _pointRuleService;

    // Constructor for dependency injection
    public AdminRewardsController(IPointRuleService pointRuleService)
    {
        _pointRuleService = pointRuleService;
    }

    /// <summary>
    /// Retrieves all reward point rules configured in the system
    /// </summary>
    /// <returns>List of point rules</returns>
    [HttpGet("rules")]
    public async Task<IActionResult> GetRules()
    {
        // Fetch all rules from service layer
        var result = await _pointRuleService.GetAllAsync();

        // Return standardized response
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Creates a new reward point rule (Admin configuration)
    /// </summary>
    /// <param name="dto">Point rule details (conditions, points, etc.)</param>
    /// <returns>Created rule</returns>
    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule([FromBody] CreatePointRuleRequestDto dto)
    {
        // Call service to create a new rule
        var result = await _pointRuleService.CreateAsync(dto);

        // Return success response
        return Ok(ApiResponse<object>.Ok(result, "Point rule created."));
    }

    /// <summary>
    /// Deactivates a point rule (soft delete, prevents further usage)
    /// </summary>
    /// <param name="ruleId">Unique identifier of the rule</param>
    /// <returns>Confirmation message</returns>
    [HttpDelete("rules/{ruleId}")]
    public async Task<IActionResult> DeactivateRule(Guid ruleId)
    {
        // Calls service to deactivate the rule instead of deleting permanently
        await _pointRuleService.DeactivateAsync(ruleId);

        // Return success message
        return Ok(ApiResponse<object>.Ok(null, "Point rule deactivated."));
    }
}