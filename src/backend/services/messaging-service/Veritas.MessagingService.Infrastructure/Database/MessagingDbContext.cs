using Microsoft.EntityFrameworkCore;
using Veritas.MessagingService.Application.Persistence;
using Veritas.MessagingService.Domain.Entities;
using Veritas.MessagingService.Domain.Types;

namespace Veritas.MessagingService.Infrastructure.Database;

/// <summary>
/// EF Core DbContext for the Messaging module.
/// </summary>
public sealed class MessagingDbContext(DbContextOptions<MessagingDbContext> options)
    : DbContext(options), IMessagingDbContext
{
    /// <inheritdoc />
    public DbSet<SmtpSettings> SmtpSettings => Set<SmtpSettings>();

    /// <inheritdoc />
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("messaging");

        modelBuilder.Entity<SmtpSettings>(entity =>
        {
            entity.ToTable("smtp_settings");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasMaxLength(64);
            entity.Property(item => item.Host).IsRequired().HasMaxLength(255);
            entity.Property(item => item.TlsMode).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Username).HasMaxLength(255);
            entity.Property(item => item.ProtectedSecret).HasMaxLength(4096);
            entity.Property(item => item.FromEmail).IsRequired().HasMaxLength(320);
            entity.Property(item => item.FromName).HasMaxLength(255);
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.ToTable("email_templates");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TemplateKey).IsRequired().HasMaxLength(128);
            entity.Property(item => item.Subject).IsRequired().HasMaxLength(500);
            entity.Property(item => item.HtmlBody).IsRequired();
            entity.Property(item => item.TextBody).IsRequired();
            entity.HasIndex(item => new { item.TenantId, item.TemplateKey }).IsUnique();
            entity.HasData(
                new EmailTemplate
                {
                    Id = Guid.Parse("ad2403c4-e51c-44f8-87c5-a8ead8818065"),
                    TenantId = null,
                    TemplateKey = "admin.welcome",
                    Subject = "Welcome to Veritas, {{User.Email}}",
                    HtmlBody = "<p>Welcome {{User.DisplayName}}.</p><p>Open Veritas: {{Action.Url}}</p>",
                    TextBody = "Welcome {{User.DisplayName}}. Open Veritas: {{Action.Url}}",
                    IsEnabled = true,
                    CreatedAtUtc = new DateTime(2026, 05, 27, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 05, 27, 0, 0, 0, DateTimeKind.Utc)
                },
                new EmailTemplate
                {
                    Id = Guid.Parse("7f069c98-2a31-4e53-a3b2-383e24274f8c"),
                    TenantId = null,
                    TemplateKey = "system.smtp-test",
                    Subject = "Veritas SMTP test",
                    HtmlBody = "<p>SMTP test for {{System.InstanceName}} at {{System.TimestampUtc}}</p>",
                    TextBody = "SMTP test for {{System.InstanceName}} at {{System.TimestampUtc}}",
                    IsEnabled = true,
                    CreatedAtUtc = new DateTime(2026, 05, 27, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 05, 27, 0, 0, 0, DateTimeKind.Utc)
                });
        });
    }
}
