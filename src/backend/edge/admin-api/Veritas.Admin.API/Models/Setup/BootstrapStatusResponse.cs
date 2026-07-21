namespace Veritas.Admin.API.Models.Setup;

/// <summary>
/// Represents the current bootstrap state returned by the Admin API.
/// </summary>
/// <param name="IsConfigured">Indicates whether the installation already has at least one administrator.</param>
/// <param name="HasActiveBootstrap">Indicates whether a bootstrap session is active.</param>
/// <param name="ActiveBootstrapExpiresAtUtc">The UTC expiration time of the active bootstrap session, when present.</param>
public sealed record BootstrapStatusResponse(
    bool IsConfigured,
    bool HasActiveBootstrap,
    DateTime? ActiveBootstrapExpiresAtUtc);
