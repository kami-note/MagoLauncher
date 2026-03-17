using MagoLauncher.Domain.Entities;
using MagoLauncher.Domain.Enums;
using Xunit;

namespace MagoLauncher.Tests;

/// <summary>
/// Unit tests for VersionKind enum and library filter behavior (pure logic).
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
    public void FilterByVersionKind_WhenAll_ReturnsAllInstances()
    {
        var instances = new[]
        {
            CreateInstance(VersionKind.Stable),
            CreateInstance(VersionKind.Modpack),
        };
        var filtered = FilterByVersionKind(instances, VersionKind.All);
        Assert.Equal(2, filtered.Count());
    }

    [Fact]
    public void FilterByVersionKind_WhenSpecificKind_ReturnsOnlyMatching()
    {
        var instances = new[]
        {
            CreateInstance(VersionKind.Stable),
            CreateInstance(VersionKind.Modpack),
            CreateInstance(VersionKind.Stable),
        };
        var filtered = FilterByVersionKind(instances, VersionKind.Stable).ToList();
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

    private static IEnumerable<MinecraftInstance> FilterByVersionKind(
        IEnumerable<MinecraftInstance> instances,
        VersionKind filter)
    {
        if (filter == VersionKind.All) return instances;
        return instances.Where(i => i.VersionKind == filter);
    }
}
