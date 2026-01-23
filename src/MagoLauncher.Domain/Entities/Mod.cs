namespace MagoLauncher.Domain.Entities;

/// <summary>
/// Representa um mod do Minecraft
/// </summary>
public class Mod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public string? FilePath { get; set; }
    public string? IconUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ProjectId { get; set; } // ID do CurseForge/Modrinth
    public bool IsEnabled { get; set; } = true;
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
}
