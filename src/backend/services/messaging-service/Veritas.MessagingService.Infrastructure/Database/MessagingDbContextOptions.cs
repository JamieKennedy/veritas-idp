namespace Veritas.MessagingService.Infrastructure.Database;

/// <summary>
/// Messaging DbContext constants.
/// </summary>
public static class MessagingDbContextOptions
{
    /// <summary>
    /// EF Core migration history table used by the Messaging module.
    /// </summary>
    public const string MigrationsHistoryTable = "__EFMigrationsHistory_Messaging";
}
