using MagoLauncher.Domain.Entities;
using MagoLauncher.Domain.Enums;

namespace MagoLauncher.Domain;

/// <summary>
/// Applies version-kind filtering to Minecraft instances. Single place for filter logic (DRY).
/// Used by presentation layer and covered by unit tests.
/// </summary>
public static class VersionKindFilter
{
    /// <summary>
    /// Filters instances by version kind. When filter is <see cref="VersionKind.All"/>, returns all instances.
    /// </summary>
    public static IEnumerable<MinecraftInstance> Apply(
        IEnumerable<MinecraftInstance> instances,
        VersionKind filter)
    {
        if (filter == VersionKind.All)
            return instances;
        return instances.Where(i => i.VersionKind == filter);
    }
}
