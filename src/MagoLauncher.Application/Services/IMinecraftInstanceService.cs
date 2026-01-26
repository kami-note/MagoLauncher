using MagoLauncher.Domain.Entities;
using System.Diagnostics;
using System.Threading.Tasks;

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
    Task DeleteInstanceAsync(MinecraftInstance instance);
    Task<Process?> LaunchInstanceAsync(MinecraftInstance instance, string playerName, int maxRamMb, Action<string>? outputCallback = null);
}
