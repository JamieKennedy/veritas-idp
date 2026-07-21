using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veritas.PlatformService.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class NoEmailBootstrapSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptCount",
                schema: "platform",
                table: "BootstrapSessions");

            migrationBuilder.DropColumn(
                name: "OtpHash",
                schema: "platform",
                table: "BootstrapSessions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "VerifiedAtUtc",
                schema: "platform",
                table: "BootstrapSessions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAtUtc",
                schema: "platform",
                table: "BootstrapSessions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "VerifiedAtUtc",
                schema: "platform",
                table: "BootstrapSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAtUtc",
                schema: "platform",
                table: "BootstrapSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                schema: "platform",
                table: "BootstrapSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OtpHash",
                schema: "platform",
                table: "BootstrapSessions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
