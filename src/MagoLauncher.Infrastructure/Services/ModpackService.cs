using System.IO.Compression;
using System.Text.Json;
using CmlLib.Core;
using MagoLauncher.Application.DTOs;
using MagoLauncher.Application.Services;
using MagoLauncher.Domain.Entities;

namespace MagoLauncher.Infrastructure.Services;

public class ModpackService : IModpackService
{
    private readonly MinecraftPath _minecraftPath;
    private readonly HttpClient _httpClient;

    public ModpackService(HttpClient httpClient)
    {
        _minecraftPath = new MinecraftPath();
        _httpClient = httpClient;
    }

    public async Task InstallModpackAsync(ModpackApiDto modpackData, IProgress<double>? progress = null)
    {
        // 1. Determine install path: .minecraft/versions/{modpack_name}
        // Using the modpack name as the directory name is common, but might need sanitization or slug usage.
        // The user existing instances seem to use Name. Let's use Slug for folder name to be safe or Name if preferred.
        // User requested: "a instancia deve ser baixada e colocado na pasta version do .minecraft."

        // Let's use the Name but sterilized for path, or just Slug. Slug is safer. 
        // But existing instances in MinecraftInstanceService use "Name". 
        // Let's use modpackData.Name for display but ensure folder name is safe.
        // Actually, for CmlLib/Minecraft, the version ID is often the folder name.
        // If we use "All the Mods 10", folder "All the Mods 10".

        string folderName = modpackData.Name; // Or utilize a sanitizer
        string installPath = Path.Combine(_minecraftPath.Versions, folderName);

        if (!Directory.Exists(installPath))
        {
            Directory.CreateDirectory(installPath);
        }

        // 2. Download ZIP
        string zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        try
        {
            using (var response = await _httpClient.GetAsync(modpackData.DownloadLink, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1 && progress != null;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    long totalRead = 0;
                    int read;

                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read);
                        totalRead += read;

                        if (canReportProgress)
                        {
                            progress?.Report((double)totalRead / totalBytes * 100);
                        }
                    }
                }
            }

            // 3. Extract ZIP (Reporting simplified "100%" or equivalent could be done here, or split progress)
            // We need to unzip into the installPath.
            // CAUTION: Modpacks often come with an 'overrides' folder or just the files.
            // Standard CurseForge/modpack format: manifest.json + overrides folder.
            // User said: "o arquivo vem em zip. os arquivos seguem o padrão dos arquivos rastreado no launcher como esta sendo feito agora."
            // This suggests the ZIP content might match the instance folder content directly (mods, config, etc.)
            // so we unzip directly into version folder.

            ZipFile.ExtractToDirectory(zipPath, installPath, overwriteFiles: true);

            // 4. Normalize Version Files (Fix for mismatched folder/json names)
            // CmlLib/Minecraft requires Folder and JSON to have the same name.
            var jsonFiles = Directory.GetFiles(installPath, "*.json");
            var versionJsonPath = jsonFiles.FirstOrDefault(f => !Path.GetFileName(f).Equals("mago_instance.json", StringComparison.OrdinalIgnoreCase));

            if (versionJsonPath != null)
            {
                var targetName = folderName; // "All the Mods 10"

                // 4.1 Update ID inside the JSON
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(versionJsonPath);
                    // Use a simple dynamic parse or regex to avoid full object mapping if complex
                    // But typically Newtonsoft or System.Text.Json.Nodes is better. 
                    // Let's use string replacement for the "id" field to be safe and simple regarding schema
                    // assuming "id": "OLD_NAME" is at the start.
                    // Or properly with JsonNode.
                    var jsonNode = System.Text.Json.Nodes.JsonNode.Parse(jsonContent);
                    if (jsonNode != null)
                    {
                        var oldId = jsonNode["id"]?.ToString();
                        // Also check if jar exists with old ID pattern
                        var jarFiles = Directory.GetFiles(installPath, "*.jar");
                        var versionJarPath = jarFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(oldId, StringComparison.OrdinalIgnoreCase) || Path.GetFileNameWithoutExtension(f).Equals(Path.GetFileNameWithoutExtension(versionJsonPath), StringComparison.OrdinalIgnoreCase));

                        jsonNode["id"] = targetName;
                        await File.WriteAllTextAsync(versionJsonPath, jsonNode.ToString());

                        // 4.2 Rename JSON file
                        var newJsonPath = Path.Combine(installPath, $"{targetName}.json");
                        if (!versionJsonPath.Equals(newJsonPath, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Move(versionJsonPath, newJsonPath, true);
                        }

                        // 4.3 Rename JAR file if it exists and matches pattern
                        if (versionJarPath != null && File.Exists(versionJarPath))
                        {
                            var newJarPath = Path.Combine(installPath, $"{targetName}.jar");
                            if (!versionJarPath.Equals(newJarPath, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Move(versionJarPath, newJarPath, true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ModpackService] Error normalizing version files: {ex.Message}");
                }
            }
        }
        finally
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }

        // 4. Create ModpackMetadata
        var metadata = new ModpackMetadata
        {
            Id = Guid.NewGuid().ToString(), // The generic API doesn't seem to give a stable ID in the snippet provided other than _id or slug.
            Slug = modpackData.Slug,
            Name = modpackData.Name,
            Version = modpackData.Version,
            MinecraftVersion = modpackData.MinecraftVersion,
            ThumbnailUrl = modpackData.Thumbnail,
            Summary = modpackData.Summary,
            Changelogs = modpackData.Changelogs?.Select(c => new ModpackChangelog
            {
                Version = c.Version,
                Text = c.Text,
                UpdatedAt = c.UpdatedAt
            }).ToList()
        };

        // 5. Save mago_instance.json
        string jsonPath = Path.Combine(installPath, "mago_instance.json");
        string jsonString = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, jsonString);
    }
}
