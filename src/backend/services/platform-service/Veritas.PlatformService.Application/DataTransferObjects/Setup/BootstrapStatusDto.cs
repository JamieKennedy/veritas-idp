namespace Veritas.PlatformService.Application.DataTransferObjects.Setup;

/// <summary>
/// Describes the current platform bootstrap state for API hosts.
/// </summary>
/// <param name="IsConfigured">Indicates whether at least one administrator user exists.</param>
/// <param name="HasActiveBootstrap">Indicates whether a non-expired bootstrap session is active.</param>
/// <param name="ActiveBootstrapExpiresAtUtc">The UTC expiration time for the active bootstrap session, when one exists.</param>
public sealed record BootstrapStatusDto(
    bool IsConfigured,
    bool HasActiveBootstrap,
    DateTime? ActiveBootstrapExpiresAtUtc);
