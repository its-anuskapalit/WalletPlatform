using Microsoft.EntityFrameworkCore;
using Notification.API.Entities;
using Notification.API.Enums;

namespace Notification.API.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    public DbSet<NotificationLog>      NotificationLogs      => Set<NotificationLog>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── NotificationLog ───────────────────────────────────
        builder.Entity<NotificationLog>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasIndex(n => n.UserId);
            e.HasIndex(n => n.Type);
            e.HasIndex(n => n.CreatedAt);
            e.Property(n => n.Type).HasConversion<string>();
            e.Property(n => n.Channel).HasConversion<string>();
            e.Property(n => n.Status).HasConversion<string>();
            e.Property(n => n.Recipient).HasMaxLength(256).IsRequired();
            e.Property(n => n.Subject).HasMaxLength(300);
            e.Property(n => n.Body).HasColumnType("nvarchar(max)");
            e.Property(n => n.FailureReason).HasMaxLength(500);
            e.Property(n => n.ExternalRef).HasMaxLength(200);
        });

        // ── NotificationTemplate ──────────────────────────────
        builder.Entity<NotificationTemplate>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => new { t.Type, t.Channel }).IsUnique();
            e.Property(t => t.Type).HasConversion<string>();
            e.Property(t => t.Channel).HasConversion<string>();
            e.Property(t => t.Subject).HasMaxLength(300).IsRequired();
            e.Property(t => t.Body).HasColumnType("nvarchar(max)").IsRequired();
        });

        // ── Seed templates ────────────────────────────────────
        SeedTemplates(builder);
    }

    private static void SeedTemplates(ModelBuilder builder)
    {
        builder.Entity<NotificationTemplate>().HasData(

            // Welcome email
            new NotificationTemplate
            {
                Id        = new Guid("20000000-0000-0000-0000-000000000001"),
                Type      = NotificationType.Welcome,
                Channel   = NotificationChannel.Email,
                Subject   = "Welcome to WalletPlatform!",
                Body      = "Hi {FullName}, welcome to WalletPlatform! Your account has been created successfully. Complete your KYC to activate your wallet.",
                IsActive  = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // KYC Approved email
            new NotificationTemplate
            {
                Id        = new Guid("20000000-0000-0000-0000-000000000002"),
                Type      = NotificationType.KYCApproved,
                Channel   = NotificationChannel.Email,
                Subject   = "KYC Approved — Your wallet is now active",
                Body      = "Hi {FullName}, your KYC verification has been approved. Your wallet is now active and ready to use.",
                IsActive  = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // Transaction success email
            new NotificationTemplate
            {
                Id        = new Guid("20000000-0000-0000-0000-000000000003"),
                Type      = NotificationType.TransactionSuccess,
                Channel   = NotificationChannel.Email,
                Subject   = "Payment of ₹{Amount} sent successfully",
                Body      = "Hi {FullName}, your payment of ₹{Amount} has been processed successfully. Transaction ID: {TransactionId}.",
                IsActive  = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // Points awarded email
            new NotificationTemplate
            {
                Id        = new Guid("20000000-0000-0000-0000-000000000004"),
                Type      = NotificationType.PointsAwarded,
                Channel   = NotificationChannel.Email,
                Subject   = "You earned {Points} reward points!",
                Body      = "Hi {FullName}, you earned {Points} points from your recent transaction. Your total balance is now {TotalPoints} points. Current tier: {TierName}.",
                IsActive  = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // Tier upgrade email
            new NotificationTemplate
            {
                Id        = new Guid("20000000-0000-0000-0000-000000000005"),
                Type      = NotificationType.TierUpgrade,
                Channel   = NotificationChannel.Email,
                Subject   = "Congratulations! You reached {TierName} tier",
                Body      = "Hi {FullName}, you have been upgraded to {TierName} tier! You now earn {Multiplier}x points on every transaction.",
                IsActive  = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // Welcome SMS
            new NotificationTemplate
            {
                Id        = new Guid("20000000-0000-0000-0000-000000000006"),
                Type      = NotificationType.Welcome,
                Channel   = NotificationChannel.SMS,
                Subject   = "",
                Body      = "Welcome to WalletPlatform, {FullName}! Your account is ready. Complete KYC to start transacting.",
                IsActive  = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // Transaction success SMS
            new NotificationTemplate
            {
                Id        = new Guid("20000000-0000-0000-0000-000000000007"),
                Type      = NotificationType.TransactionSuccess,
                Channel   = NotificationChannel.SMS,
                Subject   = "",
                Body      = "WalletPlatform: ₹{Amount} sent successfully. Txn ID: {TransactionId}. Rewards: +{Points} pts.",
                IsActive  = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}