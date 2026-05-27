using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veritas.UserService.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialAdminSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InitialAdminSlot",
                schema: "users",
                table: "AdminUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_InitialAdminSlot",
                schema: "users",
                table: "AdminUsers",
                column: "InitialAdminSlot",
                unique: true,
                filter: "\"InitialAdminSlot\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdminUsers_InitialAdminSlot",
                schema: "users",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "InitialAdminSlot",
                schema: "users",
                table: "AdminUsers");
        }
    }
}
