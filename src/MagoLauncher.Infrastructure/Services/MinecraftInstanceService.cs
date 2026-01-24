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

            var instance = new MinecraftInstance
            {
                Name = version.Name,
                MinecraftVersion = version.Name, // Using the directory name/ID as version for now
                ModLoaderType = Domain.Enums.ModLoaderType.Vanilla,
                InstancePath = versionPath,
                IsInstalled = isInstalled,
            };

            // Try to load metadata
            var jsonPath = Path.Combine(versionPath, "mago_instance.json");
            if (File.Exists(jsonPath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(jsonPath);
                    var metadata = System.Text.Json.JsonSerializer.Deserialize<ModpackMetadata>(json);
                    instance.Metadata = metadata;
                    if (metadata != null)
                    {
                        // Use metadata name if available, often cleaner than folder name
                        instance.Name = metadata.Name;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MagoLauncher] Failed to load metadata for {version.Name}: {ex.Message}");
                }
            }

            instances.Add(instance);
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

    public async Task LaunchInstanceAsync(MinecraftInstance instance, string playerName, int maxRamMb, Action<string>? outputCallback = null)
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

        // FIX: NeoForge 1.21+ BootstrapLauncher Conflict
        // The error "Module named cpw.mods.bootstraplauncher was already on the JVMs ... but class-path contains it"
        // means we must remove these from the ClassPath argument in StartInfo.Arguments.

        try
        {
            var args = process.StartInfo.Arguments;
            // Regex to find "-cp <path>" or "-classpath <path>"
            // It's tricky because paths might be quoted or not.
            // Typically arguments are: ... -cp "path1;path2;..." ...

            // We can split the whole arguments string by space, but quotes make it hard.
            // Let's assume standard formatting:  -cp "..."

            var cpToken = "-cp";
            var cpIndex = args.IndexOf(cpToken);
            if (cpIndex == -1)
            {
                cpToken = "-classpath";
                cpIndex = args.IndexOf(cpToken);
            }

            if (cpIndex != -1)
            {
                // Find the start of the path string
                var pathStart = cpIndex + cpToken.Length;
                while (pathStart < args.Length && char.IsWhiteSpace(args[pathStart])) pathStart++;

                // Determine if quoted
                bool isQuoted = args[pathStart] == '"';
                int pathEnd = -1;

                if (isQuoted)
                {
                    pathStart++; // skip quote
                    pathEnd = args.IndexOf('"', pathStart);
                }
                else
                {
                    // Find next space
                    pathEnd = args.IndexOf(' ', pathStart);
                    if (pathEnd == -1) pathEnd = args.Length;
                }

                if (pathEnd != -1)
                {
                    var cpValue = args.Substring(pathStart, pathEnd - pathStart);
                    var separator = ";"; // Windows
                    var paths = cpValue.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList();

                    var filteredPaths = paths.Where(p =>
                       !p.Contains("bootstraplauncher") &&
                       !p.Contains("securejarhandler") &&
                       !p.Contains("asm-commons") &&
                       !p.Contains("asm-util") &&
                       !p.Contains("asm-analysis") &&
                       !p.Contains("asm-tree") &&
                       !p.Contains("asm-9") &&
                       !p.Contains("JarJarFileSystems")
                   ).ToList();

                    if (filteredPaths.Count < paths.Count)
                    {
                        var newCpValue = string.Join(separator, filteredPaths);

                        // Reconstruct arguments string
                        var prefix = args.Substring(0, isQuoted ? pathStart - 1 : pathStart); // include quote if quoted
                        var suffix = args.Substring(isQuoted ? pathEnd + 1 : pathEnd); // skip closing quote

                        // We need to put it back quoted if it was quoted
                        if (isQuoted)
                            process.StartInfo.Arguments = $"{prefix}\"{newCpValue}\"{suffix}";
                        else
                            process.StartInfo.Arguments = $"{prefix}{newCpValue}{suffix}";

                        outputCallback?.Invoke("[MagoLauncher] Applied NeoForge ModulePath conflict fix (Filtered CP).");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            outputCallback?.Invoke($"[WARN] Failed to apply classpath fix: {ex.Message}");
        }

        // Enable capturing of output for debugging
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

        // Start the process
        try
        {
            process.Start();
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            outputCallback?.Invoke($"[ERROR] Failed to start Java process: {ex.Message}");
            outputCallback?.Invoke("[ERROR] Please ensure Java is installed and configured correctly.");
            throw;
        }

        // Read output asynchronously
        _ = Task.Run(async () =>
        {
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                if (line != null)
                {
                    Console.WriteLine($"[MC-OUT] {line}");
                    outputCallback?.Invoke(line);
                }
            }
        });

        _ = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream)
            {
                var line = await process.StandardError.ReadLineAsync();
                if (line != null)
                {
                    Console.WriteLine($"[MC-ERR] {line}");
                    outputCallback?.Invoke($"[ERR] {line}");
                }
            }
        });

        // Wait a bit to see if the process exits immediately with an error
        await Task.Delay(5000);

        if (process.HasExited && process.ExitCode != 0)
        {
            outputCallback?.Invoke($"[ERROR] Process exited with code {process.ExitCode}");
            throw new Exception($"Minecraft exited immediately with code {process.ExitCode}");
        }
    }
}
