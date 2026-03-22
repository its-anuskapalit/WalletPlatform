using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rewards.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PointRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PointsPerRupee = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxPointsPerTxn = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RewardTiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinPoints = table.Column<int>(type: "int", nullable: false),
                    MaxPoints = table.Column<int>(type: "int", nullable: false),
                    MultiplierFactor = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BadgeColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    LifetimePoints = table.Column<int>(type: "int", nullable: false),
                    RedeemedPoints = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyAccounts_RewardTiers_TierId",
                        column: x => x.TierId,
                        principalTable: "RewardTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoyaltyAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    BalanceBefore = table.Column<int>(type: "int", nullable: false),
                    BalanceAfter = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointTransactions_LoyaltyAccounts_LoyaltyAccountId",
                        column: x => x.LoyaltyAccountId,
                        principalTable: "LoyaltyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PointRules",
                columns: new[] { "Id", "CreatedAt", "IsActive", "MaxAmount", "MaxPointsPerTxn", "MinAmount", "Name", "PointsPerRupee", "TransactionType", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000010"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, 500, 10m, "Peer Transfer Reward", 0.1m, "PeerTransfer", null },
                    { new Guid("00000000-0000-0000-0000-000000000011"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, 1000, 50m, "Merchant Pay Reward", 0.2m, "MerchantPay", null }
                });

            migrationBuilder.InsertData(
                table: "RewardTiers",
                columns: new[] { "Id", "BadgeColor", "CreatedAt", "DisplayOrder", "IsActive", "MaxPoints", "MinPoints", "MultiplierFactor", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "#CD7F32", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, 999, 0, 1.0m, "Bronze" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "#C0C0C0", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, 4999, 1000, 1.5m, "Silver" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "#FFD700", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, 19999, 5000, 2.0m, "Gold" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "#E5E4E2", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, true, -1, 20000, 3.0m, "Platinum" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyAccounts_TierId",
                table: "LoyaltyAccounts",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyAccounts_UserId",
                table: "LoyaltyAccounts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_CreatedAt",
                table: "PointTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_LoyaltyAccountId",
                table: "PointTransactions",
                column: "LoyaltyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_ReferenceId",
                table: "PointTransactions",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardTiers_Name",
                table: "RewardTiers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointRules");

            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.DropTable(
                name: "LoyaltyAccounts");

            migrationBuilder.DropTable(
                name: "RewardTiers");
        }
    }
}
