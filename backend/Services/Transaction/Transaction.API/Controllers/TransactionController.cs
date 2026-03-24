using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Transaction.API.DTOs.Request;
using Transaction.API.Services.Interfaces;
using WalletPlatform.Shared.Models;

namespace Transaction.API.Controllers;

// Marks this class as an API controller (auto validation, binding, etc.)
[ApiController]

// Base route: api/transaction
[Route("api/[controller]")]

// All endpoints require authenticated users
[Authorize]
public class TransactionController : ControllerBase
{
    // Service for handling transaction operations (payments, history, etc.)
    private readonly ITransactionService _transactionService;

    // Service for ledger-related operations (audit trail, balance derivation)
    private readonly ILedgerService _ledgerService;

    // Constructor for dependency injection
    public TransactionController(
        ITransactionService transactionService,
        ILedgerService ledgerService)
    {
        _transactionService = transactionService;
        _ledgerService      = ledgerService;
    }

    /// <summary>
    /// Initiates a peer-to-peer payment between users
    /// </summary>
    /// <param name="dto">Payment details (receiver, amount, etc.)</param>
    /// <returns>Transaction result</returns>
    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] InitiatePaymentRequestDto dto)
    {
        // Extract Idempotency Key from request header
        // Used to prevent duplicate transactions (important in financial systems)
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault()
            ?? Guid.NewGuid().ToString(); // Fallback if not provided

        // Extract authenticated user's ID from JWT
        var userId = GetUserId();

        // Call service to process payment with idempotency protection
        var result = await _transactionService.InitiatePaymentAsync(userId, idempotencyKey, dto);

        // Return standardized success response
        return Ok(ApiResponse<object>.Ok(result, "Payment processed successfully."));
    }

    /// <summary>
    /// Retrieves details of a specific transaction by ID
    /// </summary>
    /// <param name="transactionId">Transaction identifier</param>
    /// <returns>Transaction details</returns>
    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetTransaction(Guid transactionId)
    {
        // Fetch transaction details from service layer
        var result = await _transactionService.GetTransactionAsync(transactionId);

        // Return response
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Retrieves the authenticated user's transaction history (paginated)
    /// </summary>
    /// <param name="page">Page number (default = 1)</param>
    /// <param name="pageSize">Records per page (default = 20)</param>
    /// <returns>Paginated transaction list</returns>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        // Extract user ID from JWT
        var userId = GetUserId();

        // Fetch user's transaction history
        var result = await _transactionService.GetUserTransactionsAsync(userId, page, pageSize);

        // Return standardized response
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Retrieves ledger entries for the authenticated user (audit trail)
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Records per page</param>
    /// <returns>Ledger entries</returns>
    [HttpGet("ledger")]
    public async Task<IActionResult> GetLedger(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        // Extract user ID from JWT
        var userId = GetUserId();

        // Fetch ledger entries (credit/debit history)
        var result = await _ledgerService.GetAccountLedgerAsync(userId, page, pageSize);

        // Return response
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Calculates balance from ledger entries (used for audit/consistency check)
    /// </summary>
    /// <returns>Derived balance with currency</returns>
    [HttpGet("ledger/balance")]
    public async Task<IActionResult> GetLedgerBalance()
    {
        // Extract user ID
        var userId  = GetUserId();

        // Compute balance based on ledger entries (not wallet state)
        var balance = await _ledgerService.GetDerivedBalanceAsync(userId);

        // Return balance with currency info
        return Ok(ApiResponse<object>.Ok(new { balance, currency = "INR" }));
    }

    /// <summary>
    /// Extracts the authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>User ID as Guid</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if token is invalid/missing</exception>
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}