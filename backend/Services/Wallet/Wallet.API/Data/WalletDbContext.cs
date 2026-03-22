using Microsoft.EntityFrameworkCore;
using Wallet.API.Entities;

namespace Wallet.API.Data;

public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

    public DbSet<Wallet.API.Entities.Wallet> Wallets       => Set<Wallet.API.Entities.Wallet>();
    public DbSet<PaymentMethod>              PaymentMethods => Set<PaymentMethod>();
    public DbSet<WalletFreezeLog>            FreezeLogs     => Set<WalletFreezeLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Wallet ────────────────────────────────────────────
        builder.Entity<Wallet.API.Entities.Wallet>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasIndex(w => w.UserId).IsUnique();
            e.HasIndex(w => w.WalletNumber).IsUnique();

            e.Property(w => w.WalletNumber).HasMaxLength(20).IsRequired();
            e.Property(w => w.Currency).HasMaxLength(3).IsRequired();
            e.Property(w => w.Status).HasConversion<string>();

            e.Property(w => w.Balance)
             .HasColumnType("decimal(18,2)");

            e.Property(w => w.FrozenAmount)
             .HasColumnType("decimal(18,2)");

            // Ignore computed property — not a DB column
            e.Ignore(w => w.AvailableBalance);

            e.HasMany(w => w.PaymentMethods)
             .WithOne(p => p.Wallet)
             .HasForeignKey(p => p.WalletId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(w => w.FreezeLogs)
             .WithOne(f => f.Wallet)
             .HasForeignKey(f => f.WalletId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── PaymentMethod ─────────────────────────────────────
        builder.Entity<PaymentMethod>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Type).HasConversion<string>();
            e.Property(p => p.DisplayName).HasMaxLength(100).IsRequired();
            e.Property(p => p.Token).HasMaxLength(512).IsRequired();
            e.Property(p => p.Last4Digits).HasMaxLength(4);
        });

        // ── WalletFreezeLog ───────────────────────────────────
        builder.Entity<WalletFreezeLog>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasIndex(f => f.WalletId);
            e.Property(f => f.Action).HasMaxLength(20).IsRequired();
            e.Property(f => f.Reason).HasMaxLength(500).IsRequired();
        });
    }
}