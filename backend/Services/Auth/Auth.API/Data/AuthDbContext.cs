using Auth.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<KYCRecord> KYCRecords => Set<KYCRecord>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── User ──────────────────────────────────────────────
        builder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.PhoneNumber).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.PhoneNumber).HasMaxLength(20).IsRequired();
            e.Property(u => u.Role).HasConversion<string>();

            //Cascade means: when a User is deleted, automatically delete their Profile and KYCRecord.
            e.HasOne(u => u.Profile)
             .WithOne(p => p.User)
             .HasForeignKey<UserProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(u => u.KYCRecord)
             .WithOne(k => k.User)
             .HasForeignKey<KYCRecord>(k => k.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(u => u.RefreshTokens)
             .WithOne(t => t.User)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── UserProfile ───────────────────────────────────────
        builder.Entity<UserProfile>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            e.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        });

        // ── KYCRecord ─────────────────────────────────────────
        builder.Entity<KYCRecord>(e =>
        {
            e.HasKey(k => k.Id);
            e.HasIndex(k => k.UserId).IsUnique();
            e.Property(k => k.DocumentType).HasMaxLength(50).IsRequired();
            e.Property(k => k.DocumentNumber).HasMaxLength(100).IsRequired();
            e.Property(k => k.Status).HasConversion<string>();
        });

        // ── RefreshToken ──────────────────────────────────────
        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Token).IsUnique();
            e.Property(t => t.Token).HasMaxLength(512).IsRequired();
        });

        // ── AuditLog ──────────────────────────────────────────
        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.UserId);
            e.HasIndex(a => a.CreatedAt);
            e.Property(a => a.Action).HasMaxLength(100).IsRequired();
            e.Property(a => a.Resource).HasMaxLength(200).IsRequired();
        });
    }
}