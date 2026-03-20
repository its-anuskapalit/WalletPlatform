using Auth.API.DTOs.Request;
using Auth.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletPlatform.Shared.Models;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KYCController : ControllerBase
{
    private readonly IKYCService _kycService;

    public KYCController(IKYCService kycService)
    {
        _kycService = kycService;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] KYCSubmitRequestDto dto)
    {
        var userId = GetUserId();
        var result = await _kycService.SubmitAsync(userId, dto);
        return Ok(ApiResponse<object>.Ok(result, "KYC submitted for review."));
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _kycService.GetStatusAsync(GetUserId());
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _kycService.GetPendingAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("{kycId}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid kycId)
    {
        var adminId = GetUserId();
        var result  = await _kycService.ApproveAsync(kycId, adminId);
        return Ok(ApiResponse<object>.Ok(result, "KYC approved."));
    }

    [HttpPost("{kycId}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid kycId, [FromBody] string reason)
    {
        var adminId = GetUserId();
        var result  = await _kycService.RejectAsync(kycId, adminId, reason);
        return Ok(ApiResponse<object>.Ok(result, "KYC rejected."));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}