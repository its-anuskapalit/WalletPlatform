using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.API.DTOs.Request;
using Wallet.API.Services.Interfaces;
using WalletPlatform.Shared.Models;

namespace Wallet.API.Controllers;

// Marks this class as an API controller (auto validation, binding, etc.)
[ApiController]

// Base route: api/wallet
[Route("api/[controller]")]

// All endpoints require authentication by default
[Authorize]
public class WalletController : ControllerBase
{
    // Service dependency for wallet-related business logic
    private readonly IWalletService _walletService;

    // Constructor for dependency injection
    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Retrieves the authenticated user's wallet details
    /// </summary>
    /// <returns>Wallet balance and metadata</returns>
    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        // Extract user ID from JWT claims
        var userId = GetUserId();

        // Fetch wallet details from service layer
        var result = await _walletService.GetWalletAsync(userId);

        // Return standardized API response
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Adds money to the user's wallet (wallet top-up)
    /// </summary>
    /// <param name="dto">Funding details (amount, source, etc.)</param>
    /// <returns>Updated wallet state</returns>
    [HttpPost("fund")]
    public async Task<IActionResult> FundWallet([FromBody] FundWalletRequestDto dto)
    {
        // Extract user ID
        var userId = GetUserId();

        // Call service to add funds to wallet
        var result = await _walletService.FundWalletAsync(userId, dto);

        // Return success response with amount info
        return Ok(ApiResponse<object>.Ok(result, $"Wallet funded with {dto.Amount} INR."));
    }

    /// <summary>
    /// Withdraws money from the user's wallet
    /// </summary>
    /// <param name="dto">Withdrawal details (amount, destination, etc.)</param>
    /// <returns>Updated wallet state</returns>
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDto dto)
    {
        // Extract user ID
        var userId = GetUserId();

        // Call service to process withdrawal
        var result = await _walletService.WithdrawAsync(userId, dto);

        // Return success response
        return Ok(ApiResponse<object>.Ok(result, $"Withdrawal of {dto.Amount} INR successful."));
    }

    /// <summary>
    /// Freezes a wallet (Admin action, prevents transactions)
    /// </summary>
    /// <param name="walletId">Wallet identifier</param>
    /// <param name="dto">Freeze details (reason, metadata)</param>
    /// <returns>Updated wallet status</returns>
    [HttpPost("{walletId}/freeze")]
    [Authorize(Roles = "Admin")] // Only admins can freeze wallets
    public async Task<IActionResult> Freeze(Guid walletId, [FromBody] FreezeWalletRequestDto dto)
    {
        // Extract admin user ID from JWT
        var adminId = GetUserId();

        // Call service to freeze wallet
        var result  = await _walletService.FreezeWalletAsync(walletId, adminId, dto);

        // Return success message
        return Ok(ApiResponse<object>.Ok(result, "Wallet frozen successfully."));
    }

    /// <summary>
    /// Unfreezes a wallet (Admin action, restores transaction capability)
    /// </summary>
    /// <param name="walletId">Wallet identifier</param>
    /// <param name="dto">Unfreeze details (reason, metadata)</param>
    /// <returns>Updated wallet status</returns>
    [HttpPost("{walletId}/unfreeze")]
    [Authorize(Roles = "Admin")] // Only admins can unfreeze wallets
    public async Task<IActionResult> Unfreeze(Guid walletId, [FromBody] FreezeWalletRequestDto dto)
    {
        // Extract admin user ID
        var adminId = GetUserId();

        // Call service to unfreeze wallet
        var result  = await _walletService.UnfreezeWalletAsync(walletId, adminId, dto);

        // Return success response
        return Ok(ApiResponse<object>.Ok(result, "Wallet unfrozen successfully."));
    }

    /// <summary>
    /// Extracts authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>User ID as Guid</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if token is invalid/missing</exception>
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}