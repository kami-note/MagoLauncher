using MagoLauncher.Domain.Enums;

namespace MagoLauncher.Domain;

/// <summary>
/// Maps manifest/launcher version type strings to <see cref="VersionKind"/>.
/// Used by infrastructure and testable without CmlLib.
/// </summary>
public static class VersionKindMapper
{
    /// <summary>
    /// Maps a manifest version type string (e.g. from Mojang/CmlLib) to <see cref="VersionKind"/>.
    /// </summary>
    /// <param name="type">Version type string (e.g. "release", "snapshot", "old_beta", "old_alpha").</param>
    /// <returns>Stable, Snapshot, Beta, Alpha, or Especial for unknown.</returns>
    public static VersionKind MapFromManifestType(string? type)
    {
        var value = type?.Trim() ?? string.Empty;
        return value switch
        {
            "Release" or "release" => VersionKind.Stable,
            "Snapshot" or "snapshot" => VersionKind.Snapshot,
            "OldBeta" or "old_beta" => VersionKind.Beta,
            "OldAlpha" or "old_alpha" => VersionKind.Alpha,
            _ => VersionKind.Especial
        };
    }
}
