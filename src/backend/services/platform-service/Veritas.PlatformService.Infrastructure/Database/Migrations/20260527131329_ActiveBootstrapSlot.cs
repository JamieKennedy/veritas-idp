using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veritas.PlatformService.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ActiveBootstrapSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveBootstrapSlot",
                schema: "platform",
                table: "BootstrapSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapSessions_ActiveBootstrapSlot",
                schema: "platform",
                table: "BootstrapSessions",
                column: "ActiveBootstrapSlot",
                unique: true,
                filter: "\"ActiveBootstrapSlot\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BootstrapSessions_ActiveBootstrapSlot",
                schema: "platform",
                table: "BootstrapSessions");

            migrationBuilder.DropColumn(
                name: "ActiveBootstrapSlot",
                schema: "platform",
                table: "BootstrapSessions");
        }
    }
}
