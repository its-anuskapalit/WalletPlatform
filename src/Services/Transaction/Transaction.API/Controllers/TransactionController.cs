using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Transaction.API.DTOs.Request;
using Transaction.API.Services.Interfaces;
using WalletPlatform.Shared.Models;

namespace Transaction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILedgerService      _ledgerService;

    public TransactionController(
        ITransactionService transactionService,
        ILedgerService ledgerService)
    {
        _transactionService = transactionService;
        _ledgerService      = ledgerService;
    }

    /// <summary>Initiate a peer-to-peer payment</summary>
    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] InitiatePaymentRequestDto dto)
    {
        // Idempotency key must come from the client header
        // This is how we detect duplicate requests
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        var userId = GetUserId();
        var result = await _transactionService.InitiatePaymentAsync(userId, idempotencyKey, dto);

        return Ok(ApiResponse<object>.Ok(result, "Payment processed successfully."));
    }

    /// <summary>Get a single transaction by ID</summary>
    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetTransaction(Guid transactionId)
    {
        var result = await _transactionService.GetTransactionAsync(transactionId);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Get current user's transaction history</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _transactionService.GetUserTransactionsAsync(userId, page, pageSize);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Get ledger entries for the current user</summary>
    [HttpGet("ledger")]
    public async Task<IActionResult> GetLedger(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _ledgerService.GetAccountLedgerAsync(userId, page, pageSize);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Get ledger-derived balance (audit check vs wallet balance)</summary>
    [HttpGet("ledger/balance")]
    public async Task<IActionResult> GetLedgerBalance()
    {
        var userId  = GetUserId();
        var balance = await _ledgerService.GetDerivedBalanceAsync(userId);
        return Ok(ApiResponse<object>.Ok(new { balance, currency = "INR" }));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}