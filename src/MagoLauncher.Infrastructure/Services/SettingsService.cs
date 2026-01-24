using MagoLauncher.Application.Services;
using MagoLauncher.Domain.Entities;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MagoLauncher.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public SettingsService()
    {
        _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public async Task<GlobalSettings> LoadSettingsAsync()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new GlobalSettings();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsFilePath);
            return JsonSerializer.Deserialize<GlobalSettings>(json, _jsonOptions) ?? new GlobalSettings();
        }
        catch (Exception)
        {
            // If error reading, return defaults
            return new GlobalSettings();
        }
    }

    public async Task SaveSettingsAsync(GlobalSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            // Log or handle error? For now, we might just ignore or let it bubble if critical.
            // Given the trace logger exists in App.axaml, we could potentially use it if exposed, 
            // but for infrastructure, maybe just Console/Debug for now.
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
}
