using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Notification.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExternalRef = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "Body", "Channel", "CreatedAt", "IsActive", "Subject", "Type" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "Hi {FullName}, welcome to WalletPlatform! Your account has been created successfully. Complete your KYC to activate your wallet.", "Email", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Welcome to WalletPlatform!", "Welcome" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "Hi {FullName}, your KYC verification has been approved. Your wallet is now active and ready to use.", "Email", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "KYC Approved — Your wallet is now active", "KYCApproved" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "Hi {FullName}, your payment of ₹{Amount} has been processed successfully. Transaction ID: {TransactionId}.", "Email", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Payment of ₹{Amount} sent successfully", "TransactionSuccess" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "Hi {FullName}, you earned {Points} points from your recent transaction. Your total balance is now {TotalPoints} points. Current tier: {TierName}.", "Email", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "You earned {Points} reward points!", "PointsAwarded" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "Hi {FullName}, you have been upgraded to {TierName} tier! You now earn {Multiplier}x points on every transaction.", "Email", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Congratulations! You reached {TierName} tier", "TierUpgrade" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), "Welcome to WalletPlatform, {FullName}! Your account is ready. Complete KYC to start transacting.", "SMS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "", "Welcome" },
                    { new Guid("20000000-0000-0000-0000-000000000007"), "WalletPlatform: ₹{Amount} sent successfully. Txn ID: {TransactionId}. Rewards: +{Points} pts.", "SMS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "", "TransactionSuccess" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_CreatedAt",
                table: "NotificationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_Type",
                table: "NotificationLogs",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_UserId",
                table: "NotificationLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Type_Channel",
                table: "NotificationTemplates",
                columns: new[] { "Type", "Channel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");
        }
    }
}
