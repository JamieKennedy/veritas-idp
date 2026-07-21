using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veritas.UserService.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AdminUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.RenameTable(
                name: "AdminUsers",
                newName: "AdminUsers",
                newSchema: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "AdminUsers",
                schema: "users",
                newName: "AdminUsers");
        }
    }
}
