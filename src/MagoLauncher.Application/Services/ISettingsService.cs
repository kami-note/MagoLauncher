using MagoLauncher.Domain.Entities;
using System.Threading.Tasks;

namespace MagoLauncher.Application.Services;

public interface ISettingsService
{
    Task<GlobalSettings> LoadSettingsAsync();
    Task SaveSettingsAsync(GlobalSettings settings);
}
