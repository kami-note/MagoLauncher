using MagoLauncher.Domain.Entities;

namespace MagoLauncher.Domain.Interfaces;

public interface IMinecraftVersionService
{
    Task<IEnumerable<MinecraftInstance>> GetLocalVersionsAsync();
}
