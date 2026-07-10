namespace Veritas.Admin.API.Models.Setup;

/// <summary>
/// Represents the installation setup state required by administrative clients.
/// </summary>
/// <param name="IsConfigured">Indicates whether initial administrator bootstrap is complete.</param>
/// <param name="HasActiveBootstrap">Indicates whether a short-lived bootstrap session is active.</param>
/// <param name="ActiveBootstrapExpiresAtUtc">The UTC bootstrap-session expiry, or <see langword="null" /> when none is active.</param>
/// <param name="IsSmtpConfigured">Indicates whether SMTP settings have passed connectivity validation.</param>
public sealed record SetupStatusResponse(
    bool IsConfigured,
    bool HasActiveBootstrap,
    DateTime? ActiveBootstrapExpiresAtUtc,
    bool IsSmtpConfigured);
