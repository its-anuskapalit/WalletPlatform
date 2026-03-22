using Microsoft.EntityFrameworkCore;
using Rewards.API.Entities;

namespace Rewards.API.Data;

public class RewardsDbContext : DbContext
{
    public RewardsDbContext(DbContextOptions<RewardsDbContext> options) : base(options) { }

    public DbSet<LoyaltyAccount>   LoyaltyAccounts   => Set<LoyaltyAccount>();
    public DbSet<RewardTier>       RewardTiers        => Set<RewardTier>();
    public DbSet<PointRule>        PointRules         => Set<PointRule>();
    public DbSet<PointTransaction> PointTransactions  => Set<PointTransaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── RewardTier ────────────────────────────────────────
        builder.Entity<RewardTier>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Name).IsUnique();
            e.Property(t => t.Name).HasMaxLength(50).IsRequired();
            e.Property(t => t.BadgeColor).HasMaxLength(20);
            e.Property(t => t.MultiplierFactor).HasColumnType("decimal(5,2)");
        });

        // ── PointRule ─────────────────────────────────────────
        builder.Entity<PointRule>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).HasMaxLength(100).IsRequired();
            e.Property(r => r.TransactionType).HasConversion<string>();
            e.Property(r => r.PointsPerRupee).HasColumnType("decimal(10,4)");
            e.Property(r => r.MinAmount).HasColumnType("decimal(18,2)");
            e.Property(r => r.MaxAmount).HasColumnType("decimal(18,2)");
        });

        // ── LoyaltyAccount ────────────────────────────────────
        builder.Entity<LoyaltyAccount>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.UserId).IsUnique();

            e.HasOne(a => a.Tier)
             .WithMany(t => t.Accounts)
             .HasForeignKey(a => a.TierId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(a => a.Transactions)
             .WithOne(t => t.LoyaltyAccount)
             .HasForeignKey(t => t.LoyaltyAccountId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PointTransaction ──────────────────────────────────
        builder.Entity<PointTransaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.LoyaltyAccountId);
            e.HasIndex(t => t.ReferenceId);
            e.HasIndex(t => t.CreatedAt);
            e.Property(t => t.Type).HasConversion<string>();
            e.Property(t => t.Description).HasMaxLength(300);
        });

        // ── Seed default tiers ────────────────────────────────
        // These must exist before any loyalty account is created
        var bronzeId   = new Guid("00000000-0000-0000-0000-000000000001");
        var silverId   = new Guid("00000000-0000-0000-0000-000000000002");
        var goldId     = new Guid("00000000-0000-0000-0000-000000000003");
        var platinumId = new Guid("00000000-0000-0000-0000-000000000004");

        builder.Entity<RewardTier>().HasData(
            new RewardTier
            {
                Id               = bronzeId,
                Name             = "Bronze",
                MinPoints        = 0,
                MaxPoints        = 999,
                MultiplierFactor = 1.0m,
                BadgeColor       = "#CD7F32",
                DisplayOrder     = 1,
                IsActive         = true,
                CreatedAt        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new RewardTier
            {
                Id               = silverId,
                Name             = "Silver",
                MinPoints        = 1000,
                MaxPoints        = 4999,
                MultiplierFactor = 1.5m,
                BadgeColor       = "#C0C0C0",
                DisplayOrder     = 2,
                IsActive         = true,
                CreatedAt        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new RewardTier
            {
                Id               = goldId,
                Name             = "Gold",
                MinPoints        = 5000,
                MaxPoints        = 19999,
                MultiplierFactor = 2.0m,
                BadgeColor       = "#FFD700",
                DisplayOrder     = 3,
                IsActive         = true,
                CreatedAt        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new RewardTier
            {
                Id               = platinumId,
                Name             = "Platinum",
                MinPoints        = 20000,
                MaxPoints        = -1,  // -1 = unlimited
                MultiplierFactor = 3.0m,
                BadgeColor       = "#E5E4E2",
                DisplayOrder     = 4,
                IsActive         = true,
                CreatedAt        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // ── Seed default point rules ──────────────────────────
        builder.Entity<PointRule>().HasData(
            new PointRule
            {
                Id              = new Guid("00000000-0000-0000-0000-000000000010"),
                Name            = "Peer Transfer Reward",
                TransactionType = Enums.TransactionTypeRef.PeerTransfer,
                PointsPerRupee  = 0.1m,   // 1 point per ₹10 spent
                MinAmount       = 10m,
                MaxPointsPerTxn = 500,
                IsActive        = true,
                CreatedAt       = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new PointRule
            {
                Id              = new Guid("00000000-0000-0000-0000-000000000011"),
                Name            = "Merchant Pay Reward",
                TransactionType = Enums.TransactionTypeRef.MerchantPay,
                PointsPerRupee  = 0.2m,   // 1 point per ₹5 spent
                MinAmount       = 50m,
                MaxPointsPerTxn = 1000,
                IsActive        = true,
                CreatedAt       = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}