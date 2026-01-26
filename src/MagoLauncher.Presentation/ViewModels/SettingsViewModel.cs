using CommunityToolkit.Mvvm.ComponentModel;
using MagoLauncher.Application.Services;
using System.Threading.Tasks;

namespace MagoLauncher.Presentation.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private int _maxRamMb = 4096;

    public int[] RamOptions { get; } = [2048, 4096, 6144, 8192, 12288, 16384];

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        InitializeAsync();
    }

    public SettingsViewModel()
    {
        _settingsService = null!;
    }

    private async void InitializeAsync()
    {
        var settings = await _settingsService.LoadSettingsAsync();
        MaxRamMb = settings.MaxRamMb;
    }

    partial void OnMaxRamMbChanged(int value)
    {
        _ = SaveSettingsAsync(value);
    }

    private async Task SaveSettingsAsync(int ram)
    {
        if (_settingsService == null) return;
        var settings = await _settingsService.LoadSettingsAsync();
        settings.MaxRamMb = ram;
        await _settingsService.SaveSettingsAsync(settings);
    }
}
