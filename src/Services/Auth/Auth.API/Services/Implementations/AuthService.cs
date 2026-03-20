using Auth.API.DTOs.Request;
using Auth.API.DTOs.Response;
using Auth.API.Entities;
using Auth.API.Enums;
using Auth.API.Events.Publishers;
using Auth.API.Repositories.Interfaces;
using Auth.API.Services.Interfaces;
using WalletPlatform.Shared.Events;

namespace Auth.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository  _userRepo;
    private readonly ITokenService    _tokenService;
    private readonly AuthEventPublisher _eventPublisher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepo,
        ITokenService tokenService,
        AuthEventPublisher eventPublisher,
        ILogger<AuthService> logger)
    {
        _userRepo       = userRepo;
        _tokenService   = tokenService;
        _eventPublisher = eventPublisher;
        _logger         = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, string ipAddress)
    {
        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("An account with this email already exists.");

        if (await _userRepo.PhoneExistsAsync(dto.PhoneNumber))
            throw new InvalidOperationException("An account with this phone number already exists.");

        var user = new User
        {
            Email        = dto.Email.ToLower().Trim(),
            PhoneNumber  = dto.PhoneNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = UserRole.User,
            Profile = new UserProfile
            {
                FirstName = dto.FirstName.Trim(),
                LastName  = dto.LastName.Trim()
            }
        };

        await _userRepo.CreateAsync(user);

        // Publish event → Wallet service will create wallet on receiving this
        _eventPublisher.PublishUserRegistered(new UserRegisteredEvent
        {
            UserId      = user.Id,
            Email       = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName    = $"{dto.FirstName} {dto.LastName}"
        });

        _logger.LogInformation("New user registered: {UserId} | {Email}", user.Id, user.Email);

        await _userRepo.AddAuditLogAsync(new AuditLog
        {
            UserId    = user.Id,
            Action    = "REGISTER",
            Resource  = "Auth",
            IpAddress = ipAddress,
            Success   = true
        });

        return await BuildAuthResponseAsync(user, ipAddress);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, string ipAddress)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            await _userRepo.AddAuditLogAsync(new AuditLog
            {
                UserId    = user.Id,
                Action    = "LOGIN_FAILED",
                Resource  = "Auth",
                IpAddress = ipAddress,
                Success   = false,
                Details   = "Invalid password attempt"
            });
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        _logger.LogInformation("User logged in: {UserId}", user.Id);

        await _userRepo.AddAuditLogAsync(new AuditLog
        {
            UserId    = user.Id,
            Action    = "LOGIN",
            Resource  = "Auth",
            IpAddress = ipAddress,
            Success   = true
        });

        return await BuildAuthResponseAsync(user, ipAddress);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var storedToken = await _userRepo.GetRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!storedToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token has expired or been revoked.");

        // Rotate the refresh token
        await _userRepo.RevokeRefreshTokenAsync(refreshToken);
        return await BuildAuthResponseAsync(storedToken.User, ipAddress);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _userRepo.RevokeRefreshTokenAsync(refreshToken);
    }

    public async Task<UserResponseDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return MapToUserResponse(user);
    }

    // ── Private helpers ─────────────────────────────────────────────────

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user, string ipAddress)
    {
        var accessToken   = _tokenService.GenerateAccessToken(user);
        var refreshToken  = _tokenService.GenerateRefreshToken(user.Id, ipAddress);

        await _userRepo.AddRefreshTokenAsync(refreshToken);

        return new AuthResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt    = refreshToken.ExpiresAt,
            User         = MapToUserResponse(user)
        };
    }

    private static UserResponseDto MapToUserResponse(User user) => new()
    {
        Id          = user.Id,
        Email       = user.Email,
        PhoneNumber = user.PhoneNumber,
        FullName    = user.Profile is not null
                        ? $"{user.Profile.FirstName} {user.Profile.LastName}"
                        : string.Empty,
        Role        = user.Role.ToString(),
        IsActive    = user.IsActive,
        KYCStatus   = user.KYCRecord?.Status.ToString() ?? KYCStatus.Pending.ToString()
    };
}