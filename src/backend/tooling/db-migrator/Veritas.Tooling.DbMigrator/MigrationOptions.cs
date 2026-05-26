namespace Veritas.Tooling.DbMigrator;

/// <summary>
/// Defines runtime options for database migration execution.
/// </summary>
public sealed class MigrationOptions
{
    /// <summary>
    /// Gets the configuration section name used to bind migrator options.
    /// </summary>
    public const string SectionName = "Migration";

    /// <summary>
    /// Gets or sets the maximum number of database connection attempts before migration fails.
    /// </summary>
    public int MaxConnectAttempts { get; set; } = 30;

    /// <summary>
    /// Gets or sets the delay between database connection attempts.
    /// </summary>
    public TimeSpan ConnectRetryDelay { get; set; } = TimeSpan.FromSeconds(2);
}
