using Microsoft.EntityFrameworkCore;
using Transaction.API.Data;
using Transaction.API.Entities;
using Transaction.API.Enums;
using Transaction.API.Repositories.Interfaces;

namespace Transaction.API.Repositories.Implementations;

public class LedgerRepository : ILedgerRepository
{
    private readonly TransactionDbContext _context;

    public LedgerRepository(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task<List<LedgerEntry>> GetByAccountIdAsync(
        Guid accountId, int page, int pageSize) =>
        await _context.LedgerEntries
            .Where(l => l.AccountId == accountId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<LedgerEntry>> GetByTransactionIdAsync(Guid transactionId) =>
        await _context.LedgerEntries
            .Where(l => l.TransactionId == transactionId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();

    public async Task<LedgerEntry> CreateAsync(LedgerEntry entry)
    {
        _context.LedgerEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task CreateManyAsync(IEnumerable<LedgerEntry> entries)
    {
        _context.LedgerEntries.AddRange(entries);
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetAccountBalanceAsync(Guid accountId)
    {
        // Derive balance from ledger — credits add, debits subtract
        var credits = await _context.LedgerEntries
            .Where(l => l.AccountId == accountId && l.EntryType == LedgerEntryType.Credit)
            .SumAsync(l => l.Amount);

        var debits = await _context.LedgerEntries
            .Where(l => l.AccountId == accountId && l.EntryType == LedgerEntryType.Debit)
            .SumAsync(l => l.Amount);

        return credits - debits;
    }
}