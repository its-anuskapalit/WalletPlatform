using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.API.Services.Interfaces;
using System.Security.Claims;
using WalletPlatform.Shared.Models;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>Get current user's notification history</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
        return Ok(ApiResponse<object>.Ok(result));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));
}