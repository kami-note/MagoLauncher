using MagoLauncher.Domain.Entities;

namespace MagoLauncher.Application.Services;

/// <summary>
/// Interface para serviço de mods
/// </summary>
public interface IModService
{
    Task<IEnumerable<Mod>> GetModsByInstanceAsync(Guid instanceId);
    Task InstallModAsync(Guid instanceId, Mod mod);
    Task UninstallModAsync(Guid modId);
    Task EnableModAsync(Guid modId);
    Task DisableModAsync(Guid modId);
    Task<IEnumerable<Mod>> SearchModsAsync(string query);
}
