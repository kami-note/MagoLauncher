namespace MagoLauncher.Domain.Entities;

public class ModpackChangelog
{
    public required string Version { get; set; }
    public required string Text { get; set; }
    public DateTime UpdatedAt { get; set; }
}
