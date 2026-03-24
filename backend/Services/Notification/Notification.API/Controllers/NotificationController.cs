using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.API.Services.Interfaces;
using System.Security.Claims;
using WalletPlatform.Shared.Models;

namespace Notification.API.Controllers;

// Marks this class as an API controller (enables auto model validation, binding, etc.)
[ApiController]

// Base route: api/notification
[Route("api/[controller]")]

// Ensures all endpoints in this controller require authentication
[Authorize]
public class NotificationController : ControllerBase
{
    // Service dependency to handle notification-related business logic
    private readonly INotificationService _notificationService;

    // Constructor for dependency injection
    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Retrieves the authenticated user's notification history (paginated)
    /// </summary>
    /// <param name="page">Page number (default = 1)</param>
    /// <param name="pageSize">Number of records per page (default = 20)</param>
    /// <returns>Paginated list of notifications</returns>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        // Extract user ID from JWT token
        var userId = GetUserId();

        // Fetch notifications for the user with pagination
        var result = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);

        // Return standardized API response
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Extracts the authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>User ID as Guid</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if user ID is missing in token</exception>
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}