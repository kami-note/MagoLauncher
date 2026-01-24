namespace MagoLauncher.Domain.Entities;

public class ModpackMetadata
{
    public required string Id { get; set; }
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required string MinecraftVersion { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Summary { get; set; }
    public List<ModpackChangelog>? Changelogs { get; set; }

    // Custom Configuration
    public int? MaxRamMb { get; set; }
    public bool OverrideGlobalRam { get; set; }
    public string? JavaArgs { get; set; }
}
