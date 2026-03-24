using Auth.API.DTOs.Request;
using Auth.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletPlatform.Shared.Models;

namespace Auth.API.Controllers;

// Marks this class as an API controller (automatic validation, binding, etc.)
[ApiController]

// Defines base route: api/auth
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Dependency injection of authentication service
    private readonly IAuthService _authService;

    // Constructor to initialize auth service
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user in the system
    /// </summary>
    /// <param name="dto">User registration details (email, password, etc.)</param>
    /// <returns>JWT tokens and user profile</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        // Calls service layer to handle registration logic
        var result = await _authService.RegisterAsync(dto, GetIpAddress());

        // Returns standardized API response
        return Ok(ApiResponse<object>.Ok(result, "Registration successful."));
    }

    /// <summary>
    /// Authenticates user and returns access + refresh tokens
    /// </summary>
    /// <param name="dto">Login credentials</param>
    /// <returns>JWT tokens</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        // Calls service layer for login logic
        var result = await _authService.LoginAsync(dto, GetIpAddress());

        // Returns success response with tokens
        return Ok(ApiResponse<object>.Ok(result, "Login successful."));
    }

    /// <summary>
    /// Generates a new access token using refresh token
    /// </summary>
    /// <param name="refreshToken">Valid refresh token</param>
    /// <returns>New JWT tokens</returns>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        // Calls service to validate and refresh token
        var result = await _authService.RefreshTokenAsync(refreshToken, GetIpAddress());

        // Returns new tokens
        return Ok(ApiResponse<object>.Ok(result, "Token refreshed."));
    }

    /// <summary>
    /// Logs out the user by invalidating refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token to revoke</param>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize] // Requires user to be authenticated
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        // Calls service to invalidate refresh token
        await _authService.LogoutAsync(refreshToken);

        // Returns success message
        return Ok(ApiResponse<object>.Ok((object?)null, "Logged out successfully."));
    }

    /// <summary>
    /// Retrieves the profile of the currently logged-in user
    /// </summary>
    /// <returns>User profile details</returns>
    [HttpGet("profile")]
    [Authorize] // Only accessible to authenticated users
    public async Task<IActionResult> GetProfile()
    {
        // Extract user ID from JWT claims
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Fetch user profile from service layer
        var result = await _authService.GetProfileAsync(userId);

        // Return profile data
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Retrieves the client's IP address (used for security/logging)
    /// </summary>
    /// <returns>IP address as string</returns>
    private string GetIpAddress() =>
        Request.Headers.TryGetValue("X-Forwarded-For", out var ip)
            ? ip.ToString() // Used when behind proxy/load balancer
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"; // Fallback
}