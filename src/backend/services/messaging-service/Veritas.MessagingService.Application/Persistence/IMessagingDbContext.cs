using Microsoft.EntityFrameworkCore;

using Veritas.MessagingService.Domain.Entities;

namespace Veritas.MessagingService.Application.Persistence;

/// <summary>
/// Messaging persistence port implemented by the Infrastructure DbContext.
/// </summary>
public interface IMessagingDbContext
{
    /// <summary>
    /// SMTP settings rows. The module stores one singleton row.
    /// </summary>
    DbSet<SmtpSettings> SmtpSettings
    {
        get;
    }

    /// <summary>
    /// Email templates and future tenant overrides.
    /// </summary>
    DbSet<EmailTemplate> EmailTemplates
    {
        get;
    }

    /// <summary>
    /// Persists pending changes as one atomic unit.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels persistence.</param>
    /// <returns>The number of state entries written to storage.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
