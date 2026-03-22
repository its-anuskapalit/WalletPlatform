using Auth.API.Data;
using Auth.API.Entities;
using Auth.API.Enums;
using Auth.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Repositories.Implementations;

public class KYCRepository : IKYCRepository
{
    private readonly AuthDbContext _context;

    public KYCRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<KYCRecord?> GetByUserIdAsync(Guid userId) =>
        await _context.KYCRecords.FirstOrDefaultAsync(k => k.UserId == userId);

    public async Task<KYCRecord?> GetByIdAsync(Guid id) =>
        await _context.KYCRecords.Include(k => k.User).FirstOrDefaultAsync(k => k.Id == id);

    public async Task<KYCRecord> CreateAsync(KYCRecord record)
    {
        _context.KYCRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<KYCRecord> UpdateAsync(KYCRecord record)
    {
        _context.KYCRecords.Update(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<List<KYCRecord>> GetPendingAsync() =>
        await _context.KYCRecords
            .Include(k => k.User)
            .Where(k => k.Status == KYCStatus.Submitted)
            .OrderBy(k => k.SubmittedAt)
            .ToListAsync();
}