using Microsoft.EntityFrameworkCore;
using Wallet.API.Data;
using Wallet.API.Entities;
using Wallet.API.Repositories.Interfaces;

namespace Wallet.API.Repositories.Implementations;

public class WalletRepository : IWalletRepository
{
    private readonly WalletDbContext _context;

    public WalletRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet.API.Entities.Wallet?> GetByUserIdAsync(Guid userId) =>
        await _context.Wallets
            .Include(w => w.PaymentMethods.Where(p => p.IsActive))
            .FirstOrDefaultAsync(w => w.UserId == userId);

    public async Task<Wallet.API.Entities.Wallet?> GetByIdAsync(Guid walletId) =>
        await _context.Wallets
            .Include(w => w.PaymentMethods.Where(p => p.IsActive))
            .FirstOrDefaultAsync(w => w.Id == walletId);

    public async Task<Wallet.API.Entities.Wallet?> GetByWalletNumberAsync(string number) =>
        await _context.Wallets
            .FirstOrDefaultAsync(w => w.WalletNumber == number);

    public async Task<Wallet.API.Entities.Wallet> CreateAsync(Wallet.API.Entities.Wallet wallet)
    {
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet.API.Entities.Wallet> UpdateAsync(Wallet.API.Entities.Wallet wallet)
    {
        wallet.UpdatedAt = DateTime.UtcNow;
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<PaymentMethod?> GetPaymentMethodAsync(Guid id) =>
        await _context.PaymentMethods.FindAsync(id);

    public async Task<PaymentMethod> AddPaymentMethodAsync(PaymentMethod method)
    {
        _context.PaymentMethods.Add(method);
        await _context.SaveChangesAsync();
        return method;
    }

    public async Task AddFreezeLogAsync(WalletFreezeLog log)
    {
        _context.FreezeLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}