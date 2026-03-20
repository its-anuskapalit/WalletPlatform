using Catalog.API.Entities;
using Microsoft.EntityFrameworkCore;
using Catalog.API.Enums;
namespace Catalog.API.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    public DbSet<Redemption>  Redemptions  => Set<Redemption>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── CatalogItem ───────────────────────────────────────
        builder.Entity<CatalogItem>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Category);
            e.HasIndex(c => c.IsActive);
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Description).HasMaxLength(1000);
            e.Property(c => c.Brand).HasMaxLength(100);
            e.Property(c => c.Category).HasConversion<string>();

            e.HasMany(c => c.Redemptions)
             .WithOne(r => r.CatalogItem)
             .HasForeignKey(r => r.CatalogItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Redemption ────────────────────────────────────────
        builder.Entity<Redemption>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.UserId);
            e.HasIndex(r => r.Status);
            e.HasIndex(r => r.CreatedAt);
            e.Property(r => r.Status).HasConversion<string>();
            e.Property(r => r.VoucherCode).HasMaxLength(100);
            e.Property(r => r.FailureReason).HasMaxLength(500);
        });

        // ── Seed sample catalog items ─────────────────────────
        builder.Entity<CatalogItem>().HasData(
            new CatalogItem
            {
                Id          = new Guid("10000000-0000-0000-0000-000000000001"),
                Name        = "Amazon ₹100 Voucher",
                Description = "Redeem for ₹100 off on Amazon India",
                Category    = CatalogItemCategory.Voucher,
                PointsCost  = 200,
                Brand       = "Amazon",
                StockCount  = -1,
                IsActive    = true,
                ValidFrom   = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt   = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new CatalogItem
            {
                Id          = new Guid("10000000-0000-0000-0000-000000000002"),
                Name        = "₹50 Wallet Cashback",
                Description = "Get ₹50 credited directly to your wallet",
                Category    = CatalogItemCategory.Cashback,
                PointsCost  = 100,
                Brand       = "WalletPlatform",
                StockCount  = -1,
                IsActive    = true,
                ValidFrom   = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt   = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new CatalogItem
            {
                Id          = new Guid("10000000-0000-0000-0000-000000000003"),
                Name        = "Netflix 1-Month Subscription",
                Description = "1 month Netflix Basic subscription",
                Category    = CatalogItemCategory.Subscription,
                PointsCost  = 800,
                Brand       = "Netflix",
                StockCount  = 50,
                IsActive    = true,
                ValidFrom   = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt   = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}