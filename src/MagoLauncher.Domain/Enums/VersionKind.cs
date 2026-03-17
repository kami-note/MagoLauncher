namespace MagoLauncher.Domain.Enums;

/// <summary>
/// Type of Minecraft version for library filtering.
/// Maps to Mojang/CmlLib version types and modpack instances.
/// </summary>
public enum VersionKind
{
    All,
    Stable,
    Snapshot,
    Especial,
    Beta,
    Alpha,
    Modpack
}
