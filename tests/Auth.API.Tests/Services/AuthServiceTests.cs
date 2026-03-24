using Auth.API.DTOs.Request;
using Auth.API.Entities;
using Auth.API.Enums;
using Auth.API.Events.Publishers;
using Auth.API.Repositories.Interfaces;
using Auth.API.Services.Implementations;
using Auth.API.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Auth.API.Tests.Services;

public class AuthServiceTests
{
    // ── Mocks ─────────────────────────────────────────────────────────────
    private readonly Mock<IUserRepository>      _userRepo      = new();
    private readonly Mock<IKYCRepository>       _kycRepo       = new();
    private readonly Mock<ITokenService>        _tokenService  = new();
    private readonly Mock<AuthEventPublisher>   _eventPublisher;
    private readonly Mock<ILogger<AuthService>> _logger        = new();
    private readonly AuthService                _sut;

    public AuthServiceTests()
    {
        _eventPublisher = new Mock<AuthEventPublisher>(
            new Mock<WalletPlatform.Shared.Messaging.IRabbitMQPublisher>().Object,
            new Mock<ILogger<AuthEventPublisher>>().Object);

        _sut = new AuthService(
            _userRepo.Object,
            _kycRepo.Object,
            _tokenService.Object,
            _eventPublisher.Object,
            _logger.Object);
    }

    // ── Helper builders ───────────────────────────────────────────────────
    private static RegisterRequestDto ValidRegisterDto() => new()
    {
        FirstName   = "Rahul",
        LastName    = "Sharma",
        Email       = "rahul@test.com",
        Password    = "Test@1234",
        PhoneNumber = "+919876543210"
    };

    private static User BuildUser(RegisterRequestDto dto) => new()
    {
        Id          = Guid.NewGuid(),
        Email       = dto.Email.ToLower(),
        PhoneNumber = dto.PhoneNumber,
        Role        = UserRole.User,
        IsActive    = true,
        Profile     = new UserProfile
        {
            FirstName = dto.FirstName,
            LastName  = dto.LastName
        }
    };

    // ═════════════════════════════════════════════════════════════════════
    // RegisterAsync Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
    {
        // Arrange
        var dto  = ValidRegisterDto();
        var user = BuildUser(dto);

        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.PhoneExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _userRepo.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("fake-access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "fake-refresh-token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        var result = await _sut.RegisterAsync(dto, "127.0.0.1");

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("fake-access-token");
        result.RefreshToken.Should().Be("fake-refresh-token");
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = ValidRegisterDto();
        _userRepo.Setup(r => r.EmailExistsAsync(dto.Email.ToLower())).ReturnsAsync(true);

        // Act
        Func<Task> act = () => _sut.RegisterAsync(dto, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicatePhone_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = ValidRegisterDto();
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.PhoneExistsAsync(dto.PhoneNumber)).ReturnsAsync(true);

        // Act
        Func<Task> act = () => _sut.RegisterAsync(dto, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*phone*");
    }

    [Fact]
    public async Task RegisterAsync_EmailStoredAsLowercase()
    {
        // Arrange
        var dto = ValidRegisterDto();
        dto.Email = "RAHUL@TEST.COM";

        User? savedUser = null;
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.PhoneExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => savedUser = u)
            .ReturnsAsync((User u) => u);
        _userRepo.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        _tokenService.Setup(t => t.GenerateRefreshToken(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "refresh", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        await _sut.RegisterAsync(dto, "127.0.0.1");

        // Assert
        savedUser!.Email.Should().Be("rahul@test.com");
    }

    [Fact]
    public async Task RegisterAsync_PasswordIsHashed_NotStoredAsPlainText()
    {
        // Arrange
        var dto      = ValidRegisterDto();
        User? saved  = null;

        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.PhoneExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => saved = u)
            .ReturnsAsync((User u) => u);
        _userRepo.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        _tokenService.Setup(t => t.GenerateRefreshToken(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "refresh", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        await _sut.RegisterAsync(dto, "127.0.0.1");

        // Assert
        saved!.PasswordHash.Should().NotBe(dto.Password);
        saved.PasswordHash.Should().StartWith("$2");  // BCrypt prefix
    }

    [Fact]
    public async Task RegisterAsync_PublishesUserRegisteredEvent()
    {
        // Arrange
        var dto  = ValidRegisterDto();
        var user = BuildUser(dto);

        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.PhoneExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _userRepo.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        _tokenService.Setup(t => t.GenerateRefreshToken(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "refresh", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        await _sut.RegisterAsync(dto, "127.0.0.1");

        // Assert
        _eventPublisher.Verify(
            e => e.PublishUserRegistered(It.IsAny<WalletPlatform.Shared.Events.UserRegisteredEvent>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WritesAuditLog()
    {
        // Arrange
        var dto  = ValidRegisterDto();
        var user = BuildUser(dto);

        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.PhoneExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _userRepo.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        _tokenService.Setup(t => t.GenerateRefreshToken(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "refresh", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        await _sut.RegisterAsync(dto, "127.0.0.1");

        // Assert
        _userRepo.Verify(
            r => r.AddAuditLogAsync(It.Is<AuditLog>(l => l.Action == "REGISTER" && l.Success)),
            Times.Once);
    }

    // ═════════════════════════════════════════════════════════════════════
    // LoginAsync Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var dto  = new LoginRequestDto { Email = "rahul@test.com", Password = "Test@1234" };
        var hash = BCrypt.Net.BCrypt.HashPassword("Test@1234");
        var user = new User
        {
            Id           = Guid.NewGuid(),
            Email        = "rahul@test.com",
            PasswordHash = hash,
            IsActive     = true,
            Role         = UserRole.User,
            Profile      = new UserProfile { FirstName = "Rahul", LastName = "Sharma" }
        };

        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userRepo.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken(user.Id, It.IsAny<string>()))
            .Returns(new RefreshToken { Token = "refresh-token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Act
        var result = await _sut.LoginAsync(dto, "127.0.0.1");

        // Assert
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dto  = new LoginRequestDto { Email = "rahul@test.com", Password = "WrongPassword" };
        var hash = BCrypt.Net.BCrypt.HashPassword("Test@1234");
        var user = new User { PasswordHash = hash, IsActive = true, Email = dto.Email };

        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        // Act
        Func<Task> act = () => _sut.LoginAsync(dto, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "nobody@test.com", Password = "Test@1234" };
        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        // Act
        Func<Task> act = () => _sut.LoginAsync(dto, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_WithDeactivatedAccount_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dto  = new LoginRequestDto { Email = "rahul@test.com", Password = "Test@1234" };
        var hash = BCrypt.Net.BCrypt.HashPassword("Test@1234");
        var user = new User { PasswordHash = hash, IsActive = false, Email = dto.Email };

        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        // Act
        Func<Task> act = () => _sut.LoginAsync(dto, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_FailedLogin_WritesFailedAuditLog()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "nobody@test.com", Password = "Test@1234" };
        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        // Act
        try { await _sut.LoginAsync(dto, "127.0.0.1"); } catch { }

        // Assert
        _userRepo.Verify(
            r => r.AddAuditLogAsync(It.Is<AuditLog>(l => !l.Success && l.Action == "LOGIN_FAILED")),
            Times.Once);
    }
}
