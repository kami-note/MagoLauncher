using System.Text.Json;
using MagoLauncher.Domain.Entities;
using MagoLauncher.Domain.Interfaces;

namespace MagoLauncher.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório usando JSON para persistência local
/// </summary>
public class JsonMinecraftInstanceRepository : IMinecraftInstanceRepository
{
    private readonly string _dataFilePath;
    private List<MinecraftInstance> _instances = [];

    public JsonMinecraftInstanceRepository()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MagoLauncher"
        );
        Directory.CreateDirectory(appDataPath);
        _dataFilePath = Path.Combine(appDataPath, "instances.json");
        LoadData();
    }

    private void LoadData()
    {
        if (File.Exists(_dataFilePath))
        {
            var json = File.ReadAllText(_dataFilePath);
            _instances = JsonSerializer.Deserialize<List<MinecraftInstance>>(json) ?? [];
        }
    }

    private async Task SaveDataAsync()
    {
        var json = JsonSerializer.Serialize(_instances, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_dataFilePath, json);
    }

    public Task<IEnumerable<MinecraftInstance>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<MinecraftInstance>>(_instances);
    }

    public Task<MinecraftInstance?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_instances.FirstOrDefault(i => i.Id == id));
    }

    public async Task AddAsync(MinecraftInstance instance)
    {
        _instances.Add(instance);
        await SaveDataAsync();
    }

    public async Task UpdateAsync(MinecraftInstance instance)
    {
        var index = _instances.FindIndex(i => i.Id == instance.Id);
        if (index >= 0)
        {
            _instances[index] = instance;
            await SaveDataAsync();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        _instances.RemoveAll(i => i.Id == id);
        await SaveDataAsync();
    }
}
