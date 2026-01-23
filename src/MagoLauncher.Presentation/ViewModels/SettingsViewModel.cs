using CommunityToolkit.Mvvm.ComponentModel;

namespace MagoLauncher.Presentation.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _maxRamMb = 4096;

    public int[] RamOptions { get; } = [2048, 4096, 6144, 8192, 12288, 16384];
}
