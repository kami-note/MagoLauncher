namespace MagoLauncher.Domain.Entities;

/// <summary>
/// Representa uma instância do Minecraft
/// </summary>
public class MinecraftInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string MinecraftVersion { get; set; }
    public string? ModLoaderVersion { get; set; }
    public Domain.Enums.ModLoaderType ModLoaderType { get; set; } = Domain.Enums.ModLoaderType.Vanilla;
    public string? IconPath { get; set; }
    public string? InstancePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayedAt { get; set; }
    public long PlayTimeMinutes { get; set; }
    public bool IsInstalled { get; set; } = true;
    public bool IsUpdateAvailable { get; set; }
    public List<Mod> Mods { get; set; } = [];
    public ModpackMetadata? Metadata { get; set; }
    /// <summary>
    /// Version type for library filtering (Stable, Snapshot, Beta, Alpha, Modpack, Especial).
    /// </summary>
    public Domain.Enums.VersionKind VersionKind { get; set; } = Domain.Enums.VersionKind.Stable;
}
