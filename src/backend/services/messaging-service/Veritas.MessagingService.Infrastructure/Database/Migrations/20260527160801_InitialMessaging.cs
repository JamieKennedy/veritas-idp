using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Veritas.MessagingService.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMessaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateTable(
                name: "email_templates",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HtmlBody = table.Column<string>(type: "text", nullable: false),
                    TextBody = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "smtp_settings",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    TlsMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ProtectedSecret = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    FromEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    FromName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsConfigured = table.Column<bool>(type: "boolean", nullable: false),
                    LastSuccessfulTestAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_smtp_settings", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "messaging",
                table: "email_templates",
                columns: new[] { "Id", "CreatedAtUtc", "HtmlBody", "IsEnabled", "Subject", "TemplateKey", "TenantId", "TextBody", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("7f069c98-2a31-4e53-a3b2-383e24274f8c"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), "<p>SMTP test for {{System.InstanceName}} at {{System.TimestampUtc}}</p>", true, "Veritas SMTP test", "system.smtp-test", null, "SMTP test for {{System.InstanceName}} at {{System.TimestampUtc}}", new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ad2403c4-e51c-44f8-87c5-a8ead8818065"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), "<p>Welcome {{User.DisplayName}}.</p><p>Open Veritas: {{Action.Url}}</p>", true, "Welcome to Veritas, {{User.Email}}", "admin.welcome", null, "Welcome {{User.DisplayName}}. Open Veritas: {{Action.Url}}", new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_TenantId_TemplateKey",
                schema: "messaging",
                table: "email_templates",
                columns: new[] { "TenantId", "TemplateKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_templates",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "smtp_settings",
                schema: "messaging");
        }
    }
}
