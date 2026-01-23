using MagoLauncher.Domain.Entities;

namespace MagoLauncher.Application.Services;

/// <summary>
/// Interface para serviço de instâncias do Minecraft
/// </summary>
public interface IMinecraftInstanceService
{
    Task<IEnumerable<MinecraftInstance>> GetAllInstancesAsync();
    Task<MinecraftInstance?> GetInstanceByIdAsync(Guid id);
    Task CreateInstanceAsync(MinecraftInstance instance);
    Task UpdateInstanceAsync(MinecraftInstance instance);
    Task DeleteInstanceAsync(Guid id);
    Task LaunchInstanceAsync(MinecraftInstance instance, string playerName, int maxRamMb);
}
