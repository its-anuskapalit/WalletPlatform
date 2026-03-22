using Auth.API.DTOs.Request;
using Auth.API.DTOs.Response;
using Auth.API.Entities;
using Auth.API.Enums;
using Auth.API.Events.Publishers;
using Auth.API.Repositories.Interfaces;
using Auth.API.Services.Interfaces;
using WalletPlatform.Shared.Events;

namespace Auth.API.Services.Implementations;

public class KYCService : IKYCService
{
    private readonly IKYCRepository     _kycRepo;
    private readonly AuthEventPublisher _eventPublisher;
    private readonly ILogger<KYCService> _logger;

    public KYCService(
        IKYCRepository kycRepo,
        AuthEventPublisher eventPublisher,
        ILogger<KYCService> logger)
    {
        _kycRepo        = kycRepo;
        _eventPublisher = eventPublisher;
        _logger         = logger;
    }

    public async Task<KYCResponseDto> SubmitAsync(Guid userId, KYCSubmitRequestDto dto)
    {
        var existing = await _kycRepo.GetByUserIdAsync(userId);

        if (existing is not null && existing.Status == KYCStatus.Approved)
            throw new InvalidOperationException("KYC is already approved.");

        if (existing is not null && existing.Status == KYCStatus.Submitted)
            throw new InvalidOperationException("KYC is already submitted and under review.");

        if (existing is not null)
        {
            // Resubmission after rejection
            existing.DocumentType   = dto.DocumentType;
            existing.DocumentNumber = dto.DocumentNumber;
            existing.DocumentUrl    = dto.DocumentUrl;
            existing.Status         = KYCStatus.Submitted;
            existing.RejectionReason = null;
            existing.SubmittedAt    = DateTime.UtcNow;

            var updated = await _kycRepo.UpdateAsync(existing);
            return MapToResponse(updated);
        }

        var record = new KYCRecord
        {
            UserId         = userId,
            DocumentType   = dto.DocumentType,
            DocumentNumber = dto.DocumentNumber,
            DocumentUrl    = dto.DocumentUrl,
            Status         = KYCStatus.Submitted
        };

        var created = await _kycRepo.CreateAsync(record);
        _logger.LogInformation("KYC submitted for user {UserId}", userId);

        return MapToResponse(created);
    }

    public async Task<KYCResponseDto> GetStatusAsync(Guid userId)
    {
        var record = await _kycRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("No KYC record found.");

        return MapToResponse(record);
    }

    public async Task<KYCResponseDto> ApproveAsync(Guid kycId, Guid adminId)
    {
        var record = await _kycRepo.GetByIdAsync(kycId)
            ?? throw new KeyNotFoundException("KYC record not found.");

        record.Status     = KYCStatus.Approved;
        record.ReviewedBy = adminId;
        record.ReviewedAt = DateTime.UtcNow;

        var updated = await _kycRepo.UpdateAsync(record);

        // Publish → Wallet service will unfreeze/activate wallet
        _eventPublisher.PublishKYCApproved(new KYCApprovedEvent
        {
            UserId = record.UserId
        });

        _logger.LogInformation("KYC approved for user {UserId} by admin {AdminId}",
            record.UserId, adminId);

        return MapToResponse(updated);
    }

    public async Task<KYCResponseDto> RejectAsync(Guid kycId, Guid adminId, string reason)
    {
        var record = await _kycRepo.GetByIdAsync(kycId)
            ?? throw new KeyNotFoundException("KYC record not found.");

        record.Status          = KYCStatus.Rejected;
        record.ReviewedBy      = adminId;
        record.ReviewedAt      = DateTime.UtcNow;
        record.RejectionReason = reason;

        var updated = await _kycRepo.UpdateAsync(record);
        return MapToResponse(updated);
    }

    public async Task<List<KYCResponseDto>> GetPendingAsync()
    {
        var records = await _kycRepo.GetPendingAsync();
        return records.Select(MapToResponse).ToList();
    }

    private static KYCResponseDto MapToResponse(KYCRecord r) => new()
    {
        Id              = r.Id,
        DocumentType    = r.DocumentType,
        Status          = r.Status.ToString(),
        RejectionReason = r.RejectionReason,
        SubmittedAt     = r.SubmittedAt,
        ReviewedAt      = r.ReviewedAt
    };
}