using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
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
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _playerName = "MagoPlayer";

    [ObservableProperty]
    private string _searchText = "";

    public enum FilterType { All, Installed, NotInstalled }

    [ObservableProperty]
    private FilterType _filterOption = FilterType.All;

    partial void OnFilterOptionChanged(FilterType value) => FilterInstances();

    partial void OnSearchTextChanged(string value)
    {
        FilterInstances();
    }

    private readonly List<MinecraftInstance> _allInstances = [];

    public ObservableCollection<MinecraftInstance> Instances { get; } = [];

    private readonly MagoLauncher.Application.Services.IMinecraftInstanceService _instanceService;
    private readonly HomeViewModel _homePage;
    private readonly SettingsViewModel _settingsPage;

    public MainWindowViewModel(MagoLauncher.Application.Services.IMinecraftInstanceService instanceService)
    {
        _instanceService = instanceService;

        _homePage = new HomeViewModel(Instances);
        _settingsPage = new SettingsViewModel();
        _currentPage = _homePage;

        Initialize();
    }

    public MainWindowViewModel()
    {
        // Constructor for design-time preview
        _instanceService = null!;
        _homePage = new HomeViewModel(Instances);
        _settingsPage = new SettingsViewModel();
        _currentPage = _homePage;
    }

    [ObservableProperty]
    private MinecraftInstance? _selectedInstance;

    private async void Initialize()
    {
        await LoadInstances();
    }

    private async Task LoadInstances()
    {
        if (_instanceService == null) return;

        Instances.Clear();
        var versions = await _instanceService.GetAllInstancesAsync();

        _allInstances.Clear();
        _allInstances.AddRange(versions);

        FilterInstances();

        if (Instances.Count > 0 && SelectedInstance == null)
            SelectedInstance = Instances[0];
    }

    private void FilterInstances()
    {
        Instances.Clear();

        var query = SearchText?.Trim();

        var filtered = _allInstances.AsEnumerable();

        // 1. Filter by Search Text
        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(i => i.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        // 2. Filter by Option
        switch (FilterOption)
        {
            case FilterType.Installed:
                filtered = filtered.Where(i => i.IsInstalled);
                break;
            case FilterType.NotInstalled:
                filtered = filtered.Where(i => !i.IsInstalled);
                break;
            case FilterType.All:
            default:
                break;
        }

        foreach (var instance in filtered)
        {
            Instances.Add(instance);
        }

        if (Instances.Count > 0 && (SelectedInstance == null || !Instances.Contains(SelectedInstance)))
        {
            SelectedInstance = Instances[0];
        }
    }

    [RelayCommand]
    private void GoToHome() => CurrentPage = _homePage;

    [RelayCommand]
    private void GoToSettings() => CurrentPage = _settingsPage;

    [RelayCommand]
    private async Task LaunchGame()
    {
        if (SelectedInstance == null) return;

        IsLoading = true;
        StatusMessage = $"Iniciando {SelectedInstance.Name}...";

        // Launch functionality
        try
        {
            await _instanceService.LaunchInstanceAsync(SelectedInstance, PlayerName, _settingsPage.MaxRamMb);
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
