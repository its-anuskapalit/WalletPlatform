using Microsoft.EntityFrameworkCore;
using Transaction.API.Data;
using Transaction.API.Entities;
using Transaction.API.Enums;
using Transaction.API.Repositories.Interfaces;

namespace Transaction.API.Repositories.Implementations;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _context;

    public TransactionRepository(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction.API.Entities.Transaction?> GetByIdAsync(Guid id) =>
        await _context.Transactions
            .Include(t => t.LedgerEntries)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Transaction.API.Entities.Transaction?> GetByIdempotencyKeyAsync(
        string key, Guid userId) =>
        await _context.Transactions
            .Include(t => t.LedgerEntries)
            .FirstOrDefaultAsync(t =>
                t.IdempotencyKey == key && t.SenderId == userId);

    public async Task<Transaction.API.Entities.Transaction> CreateAsync(
        Transaction.API.Entities.Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction.API.Entities.Transaction> UpdateStatusAsync(
        Guid id, TransactionStatus status, string? failureReason = null)
    {
        var transaction = await _context.Transactions.FindAsync(id)
            ?? throw new KeyNotFoundException($"Transaction {id} not found.");

        transaction.Status = status;

        if (status == TransactionStatus.Completed)
            transaction.CompletedAt = DateTime.UtcNow;

        if (status == TransactionStatus.Reversed)
            transaction.ReversedAt = DateTime.UtcNow;

        if (failureReason is not null)
            transaction.FailureReason = failureReason;

        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<List<Transaction.API.Entities.Transaction>> GetByUserIdAsync(
        Guid userId, int page, int pageSize) =>
        await _context.Transactions
            .Where(t => t.SenderId == userId || t.RecipientId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.LedgerEntries)
            .ToListAsync();

    public async Task<IdempotencyRecord?> GetIdempotencyRecordAsync(
        string key, Guid userId) =>
        await _context.IdempotencyKeys
            .FirstOrDefaultAsync(i =>
                i.IdempotencyKey == key &&
                i.UserId == userId &&
                i.ExpiresAt > DateTime.UtcNow);

    public async Task SaveIdempotencyRecordAsync(IdempotencyRecord record)
    {
        _context.IdempotencyKeys.Add(record);
        await _context.SaveChangesAsync();
    }
}