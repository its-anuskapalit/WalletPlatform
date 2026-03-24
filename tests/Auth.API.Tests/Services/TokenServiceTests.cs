using Auth.API.Entities;
using Auth.API.Enums;
using Auth.API.Services.Implementations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.API.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly IConfiguration _config;

    public TokenServiceTests()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"]           = "WalletPlatform_SuperSecret_JWT_Key_2024_MustBe32CharsMin!!",
            ["Jwt:Issuer"]        = "WalletPlatform.Auth",
            ["Jwt:Audience"]      = "WalletPlatform.Client",
            ["Jwt:ExpiryMinutes"] = "60"
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        _sut = new TokenService(_config);
    }

    private static User BuildUser() => new()
    {
        Id          = Guid.NewGuid(),
        Email       = "rahul@test.com",
        PhoneNumber = "+919876543210",
        Role        = UserRole.User,
        IsActive    = true,
        Profile     = new UserProfile { FirstName = "Rahul", LastName = "Sharma" }
    };

    // ═════════════════════════════════════════════════════════════════════
    // GenerateAccessToken Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var user   = BuildUser();
        var result = _sut.GenerateAccessToken(user);
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwtFormat()
    {
        var user   = BuildUser();
        var result = _sut.GenerateAccessToken(user);

        // JWT has exactly 3 parts separated by dots
        result.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserIdInSubClaim()
    {
        var user   = BuildUser();
        var token  = _sut.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt    = handler.ReadJwtToken(token);

        jwt.Subject.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ContainsEmailClaim()
    {
        var user   = BuildUser();
        var token  = _sut.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt    = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
    }

    [Fact]
    public void GenerateAccessToken_ContainsRoleClaim()
    {
        var user   = BuildUser();
        var token  = _sut.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt    = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" &&
            c.Value == "User");
    }

    [Fact]
    public void GenerateAccessToken_ContainsJtiClaim()
    {
        var user   = BuildUser();
        var token  = _sut.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt    = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateAccessToken_TwoCallsProduceDifferentJti()
    {
        var user   = BuildUser();
        var token1 = _sut.GenerateAccessToken(user);
        var token2 = _sut.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jti1   = handler.ReadJwtToken(token1).Id;
        var jti2   = handler.ReadJwtToken(token2).Id;

        jti1.Should().NotBe(jti2); // each token has unique JTI
    }

    [Fact]
    public void GenerateAccessToken_ExpiresInApproximately60Minutes()
    {
        var user    = BuildUser();
        var token   = _sut.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt     = handler.ReadJwtToken(token);
        var expiry  = jwt.ValidTo;

        expiry.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(10));
    }

    // ═════════════════════════════════════════════════════════════════════
    // GenerateRefreshToken Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyToken()
    {
        var userId = Guid.NewGuid();
        var result = _sut.GenerateRefreshToken(userId, "127.0.0.1");
        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateRefreshToken_ExpiresIn7Days()
    {
        var userId = Guid.NewGuid();
        var result = _sut.GenerateRefreshToken(userId, "127.0.0.1");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GenerateRefreshToken_IsNotRevokedByDefault()
    {
        var userId = Guid.NewGuid();
        var result = _sut.GenerateRefreshToken(userId, "127.0.0.1");
        result.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void GenerateRefreshToken_IsActiveByDefault()
    {
        var userId = Guid.NewGuid();
        var result = _sut.GenerateRefreshToken(userId, "127.0.0.1");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void GenerateRefreshToken_TwoCallsProduceDifferentTokens()
    {
        var userId  = Guid.NewGuid();
        var token1  = _sut.GenerateRefreshToken(userId, "127.0.0.1");
        var token2  = _sut.GenerateRefreshToken(userId, "127.0.0.1");
        token1.Token.Should().NotBe(token2.Token);
    }

    [Fact]
    public void GenerateRefreshToken_StoresIpAddress()
    {
        var userId = Guid.NewGuid();
        var result = _sut.GenerateRefreshToken(userId, "192.168.1.1");
        result.CreatedByIp.Should().Be("192.168.1.1");
    }
}
