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

            var versionPath = Path.Combine(_path.Versions, version.Name);
            bool isInstalled = Directory.Exists(versionPath);

            instances.Add(new MinecraftInstance
            {
                Name = version.Name,
                MinecraftVersion = version.Name, // Using the directory name/ID as version for now
                ModLoaderType = Domain.Enums.ModLoaderType.Vanilla,
                InstancePath = versionPath,
                IsInstalled = isInstalled,
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

        // For modpacks, the game directory should be the version folder itself
        // This is where mods/, config/, saves/ are located
        var versionPath = Path.Combine(_path.Versions, instance.MinecraftVersion);
        var modsFolder = Path.Combine(versionPath, "mods");

        MinecraftLauncher launcherToUse = _launcher;

        // If this version has a 'mods' folder, it's a modpack - create a launcher with that game path
        if (Directory.Exists(modsFolder))
        {
            var modpackPath = new MinecraftPath(versionPath)
            {
                // Keep the library and asset directories pointing to the main .minecraft folder
                Library = _path.Library,
                Assets = _path.Assets,
                Versions = _path.Versions
            };
            launcherToUse = new MinecraftLauncher(modpackPath);
        }

        // Launch logic
        var process = await launcherToUse.CreateProcessAsync(instance.MinecraftVersion, launchOptions);

        // Enable capturing of output for debugging
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

        // Debug output to see what arguments are being passed
        Console.WriteLine($"[MagoLauncher] FileName: {process.StartInfo.FileName}");
        Console.WriteLine($"[MagoLauncher] Arguments: {process.StartInfo.Arguments}");
        Console.WriteLine($"[MagoLauncher] WorkingDirectory: {process.StartInfo.WorkingDirectory}");

        // Start the process
        process.Start();

        // Read output asynchronously
        _ = Task.Run(async () =>
        {
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                Console.WriteLine($"[MC-OUT] {line}");
            }
        });

        _ = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream)
            {
                var line = await process.StandardError.ReadLineAsync();
                Console.WriteLine($"[MC-ERR] {line}");
            }
        });

        // Wait a bit to see if the process exits immediately with an error
        await Task.Delay(5000);

        if (process.HasExited && process.ExitCode != 0)
        {
            throw new Exception($"Minecraft exited immediately with code {process.ExitCode}");
        }
    }
}
