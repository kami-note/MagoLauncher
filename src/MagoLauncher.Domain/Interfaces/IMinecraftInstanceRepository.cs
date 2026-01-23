using MagoLauncher.Domain.Entities;

namespace MagoLauncher.Domain.Interfaces;

/// <summary>
/// Interface para repositório de instâncias do Minecraft
/// </summary>
public interface IMinecraftInstanceRepository
{
    Task<IEnumerable<MinecraftInstance>> GetAllAsync();
    Task<MinecraftInstance?> GetByIdAsync(Guid id);
    Task AddAsync(MinecraftInstance instance);
    Task UpdateAsync(MinecraftInstance instance);
    Task DeleteAsync(Guid id);
}
