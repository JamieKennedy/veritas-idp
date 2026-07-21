using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veritas.UserService.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AdminAuthHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MfaEnabledAtUtc",
                schema: "users",
                table: "AdminUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MfaSecretProtected",
                schema: "users",
                table: "AdminUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MfaUpdatedAtUtc",
                schema: "users",
                table: "AdminUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecurityStamp",
                schema: "users",
                table: "AdminUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AdminLoginChallenges",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    ChallengeTokenHash = table.Column<string>(type: "text", nullable: false),
                    PendingMfaSecretProtected = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminLoginChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminRecoveryCodes",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminRecoveryCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminSessions",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecurityStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdleExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AbsoluteExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevocationReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminLoginChallenges_AdminUserId",
                schema: "users",
                table: "AdminLoginChallenges",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminLoginChallenges_ExpiresAtUtc",
                schema: "users",
                table: "AdminLoginChallenges",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AdminRecoveryCodes_AdminUserId",
                schema: "users",
                table: "AdminRecoveryCodes",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminSessions_AdminUserId_RevokedAtUtc",
                schema: "users",
                table: "AdminSessions",
                columns: new[] { "AdminUserId", "RevokedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminLoginChallenges",
                schema: "users");

            migrationBuilder.DropTable(
                name: "AdminRecoveryCodes",
                schema: "users");

            migrationBuilder.DropTable(
                name: "AdminSessions",
                schema: "users");

            migrationBuilder.DropColumn(
                name: "MfaEnabledAtUtc",
                schema: "users",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "MfaSecretProtected",
                schema: "users",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "MfaUpdatedAtUtc",
                schema: "users",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                schema: "users",
                table: "AdminUsers");
        }
    }
}
