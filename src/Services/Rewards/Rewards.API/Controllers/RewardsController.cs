using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rewards.API.Services.Interfaces;
using System.Security.Claims;
using WalletPlatform.Shared.Models;

namespace Rewards.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly IRewardsService _rewardsService;

    public RewardsController(IRewardsService rewardsService)
    {
        _rewardsService = rewardsService;
    }

    /// <summary>Get current user's loyalty account and tier</summary>
    [HttpGet]
    public async Task<IActionResult> GetAccount()
    {
        var userId = GetUserId();
        var result = await _rewardsService.GetAccountAsync(userId);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Get points transaction history</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _rewardsService.GetHistoryAsync(userId, page, pageSize);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Get all reward tiers</summary>
    [HttpGet("tiers")]
    public async Task<IActionResult> GetTiers()
    {
        var result = await _rewardsService.GetTiersAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}