using System;
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

    [ObservableProperty]
    private string _playerName = "MagoPlayer";

    [ObservableProperty]
    private int _maxRamMb = 4096;

    // RAM options for the dropdown
    public int[] RamOptions { get; } = [2048, 4096, 6144, 8192, 12288, 16384];

    public ObservableCollection<MinecraftInstance> Instances { get; } = [];

    private readonly MagoLauncher.Application.Services.IMinecraftInstanceService _instanceService;

    public MainWindowViewModel(MagoLauncher.Application.Services.IMinecraftInstanceService instanceService)
    {
        _instanceService = instanceService;
        Initialize();
    }

    public MainWindowViewModel()
    {
        // Constructor for design-time preview
        _instanceService = null!;
    }

    private async void Initialize()
    {
        await LoadInstances();
    }

    private async Task LoadInstances()
    {
        if (_instanceService == null) return;

        Instances.Clear();
        var versions = await _instanceService.GetAllInstancesAsync();

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

        // Launch functionality
        try
        {
            await _instanceService.LaunchInstanceAsync(SelectedInstance, PlayerName, MaxRamMb);
            StatusMessage = "Jogo iniciado com sucesso!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private void AddInstance()
    {
        StatusMessage = "Criar nova instância...";
    }
}
