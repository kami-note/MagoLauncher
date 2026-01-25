using MagoLauncher.Application.DTOs;

namespace MagoLauncher.Application.Services;

public interface IModpackService
{
    Task InstallModpackAsync(ModpackApiDto modpackData, IProgress<double>? progress = null);
    Task UpdateModpackAsync(ModpackApiDto modpackData, string instanceId, IProgress<double>? progress = null);
    Task<ModpackApiDto> GetModpackAsync(string slug);
}
