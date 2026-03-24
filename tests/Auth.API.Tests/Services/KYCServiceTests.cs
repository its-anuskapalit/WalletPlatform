using Auth.API.DTOs.Request;
using Auth.API.Entities;
using Auth.API.Enums;
using Auth.API.Events.Publishers;
using Auth.API.Repositories.Interfaces;
using Auth.API.Services.Implementations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Auth.API.Tests.Services;

public class KYCServiceTests
{
    private readonly Mock<IKYCRepository>       _kycRepo        = new();
    private readonly Mock<IUserRepository>      _userRepo       = new();
    private readonly Mock<AuthEventPublisher>   _eventPublisher;
    private readonly Mock<ILogger<KYCService>>  _logger         = new();
    private readonly KYCService                 _sut;

    public KYCServiceTests()
    {
        _eventPublisher = new Mock<AuthEventPublisher>(
            new Mock<WalletPlatform.Shared.Messaging.IRabbitMQPublisher>().Object,
            new Mock<ILogger<AuthEventPublisher>>().Object);

        _sut = new KYCService(
            _kycRepo.Object,
            _userRepo.Object,
            _eventPublisher.Object,
            _logger.Object);
    }

    private static KYCRecord PendingKYC(Guid userId) => new()
    {
        Id             = Guid.NewGuid(),
        UserId         = userId,
        DocumentType   = "Aadhaar",
        DocumentNumber = "1234-5678-9012",
        Status         = KYCStatus.Submitted
    };

    // ═════════════════════════════════════════════════════════════════════
    // SubmitAsync Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SubmitAsync_CreatesKYCRecordWithSubmittedStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto    = new KYCSubmitRequestDto
        {
            DocumentType   = "Aadhaar",
            DocumentNumber = "1234-5678-9012",
            DocumentUrl    = "https://example.com/doc.pdf"
        };

        KYCRecord? saved = null;
        _kycRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((KYCRecord?)null);
        _kycRepo.Setup(r => r.CreateAsync(It.IsAny<KYCRecord>()))
            .Callback<KYCRecord>(k => saved = k)
            .ReturnsAsync((KYCRecord k) => k);

        // Act
        await _sut.SubmitAsync(userId, dto);

        // Assert
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(KYCStatus.Submitted);
        saved.UserId.Should().Be(userId);
        saved.DocumentNumber.Should().Be(dto.DocumentNumber);
    }

    [Fact]
    public async Task SubmitAsync_WhenAlreadyApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId  = Guid.NewGuid();
        var existing = new KYCRecord { UserId = userId, Status = KYCStatus.Approved };
        var dto     = new KYCSubmitRequestDto
        {
            DocumentType   = "Aadhaar",
            DocumentNumber = "1234-5678-9012"
        };

        _kycRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existing);

        // Act
        Func<Task> act = () => _sut.SubmitAsync(userId, dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already approved*");
    }

    // ═════════════════════════════════════════════════════════════════════
    // ApproveAsync Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ApproveAsync_UpdatesStatusToApproved()
    {
        // Arrange
        var userId  = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var kyc     = PendingKYC(userId);

        var user = new User { Id = userId, Email = "rahul@test.com",
            PhoneNumber = "+91987", IsActive = true };

        _kycRepo.Setup(r => r.GetByIdAsync(kyc.Id)).ReturnsAsync(kyc);
        _kycRepo.Setup(r => r.UpdateAsync(It.IsAny<KYCRecord>())).ReturnsAsync((KYCRecord k) => k);
        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        await _sut.ApproveAsync(kyc.Id, adminId);

        // Assert
        _kycRepo.Verify(r => r.UpdateAsync(It.Is<KYCRecord>(k =>
            k.Status     == KYCStatus.Approved &&
            k.ReviewedBy == adminId &&
            k.ReviewedAt != null)), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_PublishesKYCApprovedEvent()
    {
        // Arrange
        var userId  = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var kyc     = PendingKYC(userId);
        var user    = new User { Id = userId, Email = "rahul@test.com",
            PhoneNumber = "+91987", IsActive = true };

        _kycRepo.Setup(r => r.GetByIdAsync(kyc.Id)).ReturnsAsync(kyc);
        _kycRepo.Setup(r => r.UpdateAsync(It.IsAny<KYCRecord>())).ReturnsAsync((KYCRecord k) => k);
        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        await _sut.ApproveAsync(kyc.Id, adminId);

        // Assert
        _eventPublisher.Verify(
            e => e.PublishKYCApproved(It.Is<WalletPlatform.Shared.Events.KYCApprovedEvent>(
                ev => ev.UserId == userId)),
            Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_WhenKYCNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var kycId   = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        _kycRepo.Setup(r => r.GetByIdAsync(kycId)).ReturnsAsync((KYCRecord?)null);

        // Act
        Func<Task> act = () => _sut.ApproveAsync(kycId, adminId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ═════════════════════════════════════════════════════════════════════
    // RejectAsync Tests
    // ═════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task RejectAsync_SetsStatusToRejectedWithReason()
    {
        // Arrange
        var userId  = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var kyc     = PendingKYC(userId);
        var reason  = "Document is blurry and unreadable";

        _kycRepo.Setup(r => r.GetByIdAsync(kyc.Id)).ReturnsAsync(kyc);
        _kycRepo.Setup(r => r.UpdateAsync(It.IsAny<KYCRecord>())).ReturnsAsync((KYCRecord k) => k);

        // Act
        await _sut.RejectAsync(kyc.Id, adminId, reason);

        // Assert
        _kycRepo.Verify(r => r.UpdateAsync(It.Is<KYCRecord>(k =>
            k.Status          == KYCStatus.Rejected &&
            k.RejectionReason == reason &&
            k.ReviewedBy      == adminId)), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_DoesNotPublishKYCApprovedEvent()
    {
        // Arrange
        var userId  = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var kyc     = PendingKYC(userId);

        _kycRepo.Setup(r => r.GetByIdAsync(kyc.Id)).ReturnsAsync(kyc);
        _kycRepo.Setup(r => r.UpdateAsync(It.IsAny<KYCRecord>())).ReturnsAsync((KYCRecord k) => k);

        // Act
        await _sut.RejectAsync(kyc.Id, adminId, "Invalid document");

        // Assert
        _eventPublisher.Verify(
            e => e.PublishKYCApproved(It.IsAny<WalletPlatform.Shared.Events.KYCApprovedEvent>()),
            Times.Never);
    }
}
