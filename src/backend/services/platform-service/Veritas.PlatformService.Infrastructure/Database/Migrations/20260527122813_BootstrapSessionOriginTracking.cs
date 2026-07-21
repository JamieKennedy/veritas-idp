using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veritas.PlatformService.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class BootstrapSessionOriginTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                schema: "platform",
                table: "BootstrapSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAtUtc",
                schema: "platform",
                table: "BootstrapSessions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                schema: "platform",
                table: "BootstrapSessions");

            migrationBuilder.DropColumn(
                name: "LastSeenAtUtc",
                schema: "platform",
                table: "BootstrapSessions");
        }
    }
}
