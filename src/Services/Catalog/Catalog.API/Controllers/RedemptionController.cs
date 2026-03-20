using Catalog.API.DTOs.Request;
using Catalog.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletPlatform.Shared.Models;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RedemptionController : ControllerBase
{
    private readonly IRedemptionService _redemptionService;

    public RedemptionController(IRedemptionService redemptionService)
    {
        _redemptionService = redemptionService;
    }

    /// <summary>Redeem points for a catalog item</summary>
    [HttpPost]
    public async Task<IActionResult> Redeem([FromBody] RedeemItemRequestDto dto)
    {
        var userId = GetUserId();
        var result = await _redemptionService.RedeemAsync(userId, dto);
        return Ok(ApiResponse<object>.Ok(result, "Redemption initiated successfully."));
    }

    /// <summary>Get my redemption history</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyRedemptions(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _redemptionService.GetUserRedemptionsAsync(userId, page, pageSize);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Get single redemption by ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _redemptionService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}