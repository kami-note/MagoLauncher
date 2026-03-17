using MagoLauncher.Domain;
using MagoLauncher.Domain.Enums;
using Xunit;

namespace MagoLauncher.Tests;

public class VersionKindMapperTests
{
    [Theory]
    [InlineData("release", VersionKind.Stable)]
    [InlineData("Release", VersionKind.Stable)]
    [InlineData("snapshot", VersionKind.Snapshot)]
    [InlineData("Snapshot", VersionKind.Snapshot)]
    [InlineData("old_beta", VersionKind.Beta)]
    [InlineData("OldBeta", VersionKind.Beta)]
    [InlineData("old_alpha", VersionKind.Alpha)]
    [InlineData("OldAlpha", VersionKind.Alpha)]
    public void MapFromManifestType_MapsKnownTypes_ReturnsExpectedKind(string type, VersionKind expected)
    {
        var result = VersionKindMapper.MapFromManifestType(type);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("unknown")]
    [InlineData("custom")]
    [InlineData("forge")]
    public void MapFromManifestType_UnknownOrEmpty_ReturnsEspecial(string? type)
    {
        var result = VersionKindMapper.MapFromManifestType(type);
        Assert.Equal(VersionKind.Especial, result);
    }

    [Fact]
    public void MapFromManifestType_WhitespaceOnly_ReturnsEspecial()
    {
        Assert.Equal(VersionKind.Especial, VersionKindMapper.MapFromManifestType("   "));
    }
}
