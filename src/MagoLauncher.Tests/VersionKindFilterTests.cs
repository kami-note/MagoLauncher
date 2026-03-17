using MagoLauncher.Domain;
using MagoLauncher.Domain.Entities;
using MagoLauncher.Domain.Enums;
using Xunit;

namespace MagoLauncher.Tests;

/// <summary>
/// Unit tests for VersionKind enum and VersionKindFilter (single source of truth in Domain).
/// </summary>
public class VersionKindFilterTests
{
    [Fact]
    public void VersionKind_Enum_HasExpectedValuesForLibraryFilter()
    {
        var values = (VersionKind[])Enum.GetValues(typeof(VersionKind));
        Assert.Contains(VersionKind.All, values);
        Assert.Contains(VersionKind.Stable, values);
        Assert.Contains(VersionKind.Snapshot, values);
        Assert.Contains(VersionKind.Especial, values);
        Assert.Contains(VersionKind.Beta, values);
        Assert.Contains(VersionKind.Alpha, values);
        Assert.Contains(VersionKind.Modpack, values);
        Assert.Equal(7, values.Length);
    }

    [Fact]
    public void Apply_WhenAll_ReturnsAllInstances()
    {
        var instances = new[]
        {
            CreateInstance(VersionKind.Stable),
            CreateInstance(VersionKind.Modpack),
        };
        var filtered = VersionKindFilter.Apply(instances, VersionKind.All);
        Assert.Equal(2, filtered.Count());
    }

    [Fact]
    public void Apply_WhenSpecificKind_ReturnsOnlyMatching()
    {
        var instances = new[]
        {
            CreateInstance(VersionKind.Stable),
            CreateInstance(VersionKind.Modpack),
            CreateInstance(VersionKind.Stable),
        };
        var filtered = VersionKindFilter.Apply(instances, VersionKind.Stable).ToList();
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, i => Assert.Equal(VersionKind.Stable, i.VersionKind));
    }

    private static MinecraftInstance CreateInstance(VersionKind kind)
    {
        return new MinecraftInstance
        {
            Name = kind.ToString(),
            MinecraftVersion = "1.0",
            VersionKind = kind,
        };
    }
}
