using MagoLauncher.Domain.Entities;
using MagoLauncher.Domain.Interfaces;

namespace MagoLauncher.Infrastructure.Services;

public class MinecraftVersionService : IMinecraftVersionService
{
    public Task<IEnumerable<MinecraftInstance>> GetLocalVersionsAsync()
    {
        var versions = new List<MinecraftInstance>();
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var minecraftVersionsPath = Path.Combine(appData, ".minecraft", "versions");

        if (Directory.Exists(minecraftVersionsPath))
        {
            var directories = Directory.GetDirectories(minecraftVersionsPath);
            foreach (var dir in directories)
            {
                var versionName = new DirectoryInfo(dir).Name;

                // Simple validation: check if .json exists
                if (File.Exists(Path.Combine(dir, $"{versionName}.json")))
                {
                    versions.Add(new MinecraftInstance
                    {
                        Name = versionName,
                        MinecraftVersion = versionName, // Setup basic mapping
                        ModLoaderType = Domain.Enums.ModLoaderType.Vanilla, // Default Assumption
                        InstancePath = dir,
                        IconPath = null // Standard icon
                    });
                }
            }
        }

        return Task.FromResult<IEnumerable<MinecraftInstance>>(versions);
    }
}
