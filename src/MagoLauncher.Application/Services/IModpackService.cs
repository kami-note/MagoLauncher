using MagoLauncher.Application.DTOs;

namespace MagoLauncher.Application.Services;

public interface IModpackService
{
    Task InstallModpackAsync(ModpackApiDto modpackData, IProgress<double>? progress = null);
}
