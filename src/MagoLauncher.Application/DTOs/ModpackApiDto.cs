namespace MagoLauncher.Application.DTOs;

public class ModpackApiDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public required string Version { get; set; }
    public required string MinecraftVersion { get; set; }
    public string? Author { get; set; }
    public string? Thumbnail { get; set; }
    public required string DownloadLink { get; set; }
    public List<ModpackChangelogDto>? Changelogs { get; set; }
}

public class ModpackChangelogDto
{
    public required string Version { get; set; }
    public required string Text { get; set; }
    public DateTime UpdatedAt { get; set; }
}
