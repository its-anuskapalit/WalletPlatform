using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.API.DTOs.Request;
using Wallet.API.Services.Interfaces;
using WalletPlatform.Shared.Models;

namespace Wallet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>Get the current user's wallet</summary>
    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        var userId = GetUserId();
        var result = await _walletService.GetWalletAsync(userId);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Add money to wallet</summary>
    [HttpPost("fund")]
    public async Task<IActionResult> FundWallet([FromBody] FundWalletRequestDto dto)
    {
        var userId = GetUserId();
        var result = await _walletService.FundWalletAsync(userId, dto);
        return Ok(ApiResponse<object>.Ok(result, $"Wallet funded with {dto.Amount} INR."));
    }

    /// <summary>Withdraw money from wallet</summary>
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDto dto)
    {
        var userId = GetUserId();
        var result = await _walletService.WithdrawAsync(userId, dto);
        return Ok(ApiResponse<object>.Ok(result, $"Withdrawal of {dto.Amount} INR successful."));
    }

    /// <summary>Freeze a wallet — Admin only</summary>
    [HttpPost("{walletId}/freeze")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Freeze(Guid walletId, [FromBody] FreezeWalletRequestDto dto)
    {
        var adminId = GetUserId();
        var result  = await _walletService.FreezeWalletAsync(walletId, adminId, dto);
        return Ok(ApiResponse<object>.Ok(result, "Wallet frozen successfully."));
    }

    /// <summary>Unfreeze a wallet — Admin only</summary>
    [HttpPost("{walletId}/unfreeze")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Unfreeze(Guid walletId, [FromBody] FreezeWalletRequestDto dto)
    {
        var adminId = GetUserId();
        var result  = await _walletService.UnfreezeWalletAsync(walletId, adminId, dto);
        return Ok(ApiResponse<object>.Ok(result, "Wallet unfrozen successfully."));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}