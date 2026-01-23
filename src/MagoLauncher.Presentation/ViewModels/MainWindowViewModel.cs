using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Domain.Entities;
using MagoLauncher.Domain.Enums;
using MagoLauncher.Domain.Interfaces;


namespace MagoLauncher.Presentation.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "MagoLauncher";

    [ObservableProperty]
    private string _statusMessage = "Pronto para jogar";

    [ObservableProperty]
    private MinecraftInstance? _selectedInstance;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<MinecraftInstance> Instances { get; } = [];

    private readonly IMinecraftVersionService _versionService;

    public MainWindowViewModel(IMinecraftVersionService versionService)
    {
        _versionService = versionService;
        Initialize();
    }

    public MainWindowViewModel()
    {
        // Constructor for design-time preview
        _versionService = null!;
    }

    private async void Initialize()
    {
        await LoadInstances();
    }

    private async Task LoadInstances()
    {
        if (_versionService == null) return;

        Instances.Clear();
        var versions = await _versionService.GetLocalVersionsAsync();

        foreach (var version in versions)
        {
            Instances.Add(version);
        }

        if (Instances.Count > 0)
            SelectedInstance = Instances[0];
    }

    [RelayCommand]
    private async Task LaunchGame()
    {
        if (SelectedInstance == null) return;

        IsLoading = true;
        StatusMessage = $"Iniciando {SelectedInstance.Name}...";

        // Simulação de lançamento
        await Task.Delay(1500);

        StatusMessage = "Jogo iniciado!";
        IsLoading = false;
    }

    [RelayCommand]
    private void AddInstance()
    {
        StatusMessage = "Criar nova instância...";
    }
}
