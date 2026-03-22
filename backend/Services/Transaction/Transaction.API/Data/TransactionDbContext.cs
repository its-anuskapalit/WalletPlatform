using Microsoft.EntityFrameworkCore;
using Transaction.API.Entities;

namespace Transaction.API.Data;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options)
        : base(options) { }

    public DbSet<Transaction.API.Entities.Transaction> Transactions    => Set<Transaction.API.Entities.Transaction>();
    public DbSet<LedgerEntry>                          LedgerEntries   => Set<LedgerEntry>();
    public DbSet<IdempotencyRecord>                    IdempotencyKeys => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Transaction ───────────────────────────────────────
        builder.Entity<Transaction.API.Entities.Transaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.IdempotencyKey).IsUnique();
            e.HasIndex(t => t.SenderId);
            e.HasIndex(t => t.RecipientId);
            e.HasIndex(t => t.CreatedAt);

            e.Property(t => t.Amount)
             .HasColumnType("decimal(18,2)")
             .IsRequired();

            e.Property(t => t.Currency).HasMaxLength(3).IsRequired();
            e.Property(t => t.Description).HasMaxLength(500);
            e.Property(t => t.IdempotencyKey).HasMaxLength(128).IsRequired();
            e.Property(t => t.Type).HasConversion<string>();
            e.Property(t => t.Status).HasConversion<string>();

            e.HasMany(t => t.LedgerEntries)
             .WithOne(l => l.Transaction)
             .HasForeignKey(l => l.TransactionId)
             .OnDelete(DeleteBehavior.Restrict); // Never cascade delete ledger entries
        });

        // ── LedgerEntry ───────────────────────────────────────
        builder.Entity<LedgerEntry>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasIndex(l => l.TransactionId);
            e.HasIndex(l => l.AccountId);
            e.HasIndex(l => l.CreatedAt);

            e.Property(l => l.Amount)
             .HasColumnType("decimal(18,2)")
             .IsRequired();

            e.Property(l => l.BalanceBefore).HasColumnType("decimal(18,2)");
            e.Property(l => l.BalanceAfter).HasColumnType("decimal(18,2)");
            e.Property(l => l.Currency).HasMaxLength(3).IsRequired();
            e.Property(l => l.EntryType).HasConversion<string>();
            e.Property(l => l.Description).HasMaxLength(500);
        });

        // ── IdempotencyRecord ─────────────────────────────────
        builder.Entity<IdempotencyRecord>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasIndex(i => new { i.IdempotencyKey, i.UserId }).IsUnique();
            e.Property(i => i.IdempotencyKey).HasMaxLength(128).IsRequired();
            e.Property(i => i.ResponseJson).HasColumnType("nvarchar(max)");
        });
    }
}