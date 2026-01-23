using MagoLauncher.Domain.Entities;

namespace MagoLauncher.Domain.Interfaces;

/// <summary>
/// Interface para repositório de mods
/// </summary>
public interface IModRepository
{
    Task<IEnumerable<Mod>> GetModsByInstanceIdAsync(Guid instanceId);
    Task<Mod?> GetByIdAsync(Guid id);
    Task AddAsync(Mod mod);
    Task UpdateAsync(Mod mod);
    Task DeleteAsync(Guid id);
}
