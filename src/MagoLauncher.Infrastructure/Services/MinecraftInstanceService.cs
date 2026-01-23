using System.Diagnostics;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;
using MagoLauncher.Application.Services;
using MagoLauncher.Domain.Entities;

namespace MagoLauncher.Infrastructure.Services;

public class MinecraftInstanceService : IMinecraftInstanceService
{
    private readonly MinecraftLauncher _launcher;
    private readonly MinecraftPath _path;

    public MinecraftInstanceService()
    {
        // Initializes with the default path (%appdata%/.minecraft)
        _path = new MinecraftPath();
        _launcher = new MinecraftLauncher(_path);
    }

    public async Task<IEnumerable<MinecraftInstance>> GetAllInstancesAsync()
    {
        var versions = await _launcher.GetAllVersionsAsync();
        var instances = new List<MinecraftInstance>();

        foreach (var version in versions)
        {
            // Filter basic types - CmlLib returns all version types (release, snapshot, old_beta, etc.)
            // For now, we return everything, or we could filter by specific needs

            instances.Add(new MinecraftInstance
            {
                Name = version.Name,
                MinecraftVersion = version.Name, // Using the directory name/ID as version for now
                ModLoaderType = Domain.Enums.ModLoaderType.Vanilla,
                InstancePath = Path.Combine(_path.Versions, version.Name),
                // Could try to parse specific details if needed
            });
        }

        return instances;
    }

    public Task<MinecraftInstance?> GetInstanceByIdAsync(Guid id)
    {
        // Since we are not persisting IDs yet, this is a placeholder.
        // In a real database scenario, we would lookup by ID.
        // For scanning mode, we can't reliably "get by ID" unless we persist them.
        return Task.FromResult<MinecraftInstance?>(null);
    }

    public Task CreateInstanceAsync(MinecraftInstance instance)
    {
        // Placeholder for creating a new instance
        return Task.CompletedTask;
    }

    public Task UpdateInstanceAsync(MinecraftInstance instance)
    {
        // Placeholder for updating logic
        return Task.CompletedTask;
    }

    public Task DeleteInstanceAsync(Guid id)
    {
        // Placeholder for deletion
        return Task.CompletedTask;
    }

    public async Task LaunchInstanceAsync(MinecraftInstance instance, string playerName, int maxRamMb)
    {
        // Create launch options
        var launchOptions = new MLaunchOption
        {
            MaximumRamMb = maxRamMb,
            Session = MSession.CreateOfflineSession(playerName),
        };

        // Launch logic
        var process = await _launcher.CreateProcessAsync(instance.MinecraftVersion, launchOptions);

        // Start the process
        process.Start();

        // Optional: Monitor output
        // Debug.WriteLine(process.StartInfo.Arguments);
    }
}
