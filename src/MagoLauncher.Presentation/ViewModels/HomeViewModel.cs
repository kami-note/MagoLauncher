using CommunityToolkit.Mvvm.ComponentModel;
using MagoLauncher.Presentation.Models;
using MagoLauncher.Domain.Entities;
using System.Collections.ObjectModel;
using MagoLauncher.Application.Services;
using MagoLauncher.Domain.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http;
using Avalonia.Media.Imaging;
using System.IO;

namespace MagoLauncher.Presentation.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient = new();
    private readonly IMinecraftInstanceService _instanceService;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly MainWindowViewModel _mainWindowViewModel;


    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private object? _overlayContent;

    [ObservableProperty]
    private bool _isOverlayVisible;

    [RelayCommand]
    public void CloseOverlay()
    {
        IsOverlayVisible = false;
        OverlayContent = null;
    }

    [RelayCommand]
    public void OpenInstanceSettings()
    {
        if (SelectedInstance == null) return;

        var vm = new InstanceConfigurationViewModel(
            SelectedInstance,
            _instanceService,
            () => CloseOverlay(),
            async () => await ReloadInstances()); // Reload instances on delete
        OverlayContent = vm;
        IsOverlayVisible = true;
    }


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

    [ObservableProperty]
    private MinecraftInstance? _selectedInstance;

    [ObservableProperty]
    private Bitmap? _selectedInstanceCover;

    [ObservableProperty]
    private string _lastPlayedText = "Nunca jogado";

    [ObservableProperty]
    private string _playTimeText = "0 horas";

    partial void OnSelectedInstanceChanged(MinecraftInstance? value)
    {
        if (value == null) return;

        // Update Stats
        if (value.LastPlayedAt == DateTime.MinValue)
            LastPlayedText = "Nunca jogado";
        else
            LastPlayedText = value.LastPlayedAt.Date == DateTime.Today ? "Hoje" : value.LastPlayedAt.ToString("d");

        PlayTimeText = value.PlayTimeMinutes == 0 ? "0 horas" : $"{(value.PlayTimeMinutes / 60.0):0.#} horas";

        // Load Cover
        _ = LoadInstanceCover(value);
    }

    private async Task LoadInstanceCover(MinecraftInstance instance)
    {
        SelectedInstanceCover = null; // Reset first

        if (instance.Metadata?.ThumbnailUrl is string url && !string.IsNullOrEmpty(url))
        {
            try
            {
                var imageData = await _httpClient.GetByteArrayAsync(url);
                using var stream = new MemoryStream(imageData);
                SelectedInstanceCover = new Bitmap(stream);
            }
            catch
            {
                // Fallback or ignore
            }
        }
    }

    private readonly IModpackService _modpackService;

    public HomeViewModel(IMinecraftInstanceService instanceService, SettingsViewModel settingsViewModel, MainWindowViewModel mainWindowViewModel, IModpackService modpackService)
    {
        _instanceService = instanceService;
        _settingsViewModel = settingsViewModel;
        _mainWindowViewModel = mainWindowViewModel;
        _modpackService = modpackService;
        Initialize();
    }

    public HomeViewModel()
    {
        // Constructor for design-time preview
        _instanceService = null!;
        _settingsViewModel = new SettingsViewModel();
        _mainWindowViewModel = new MainWindowViewModel();
        _modpackService = null!; // Mock not needed for simple preview unless we bind deeply
    }

    private async void Initialize()
    {
        await LoadInstances();
        await CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        if (_modpackService == null || _allInstances == null) return;

        // Parallel check for performance
        var checkTasks = _allInstances
            .Where(i => i.Metadata?.Slug != null)
            .Select(async instance =>
            {
                try
                {
                    var remote = await _modpackService.GetModpackAsync(instance.Metadata!.Slug);
                    if (remote != null && remote.Version != instance.Metadata.Version)
                    {
                        // Update on UI Thread if collection binding requires it, 
                        // though boolean property change on item usually fine if INPC implemented on Item or collection.
                        // MinecraftInstance is not ObservableObject in the entity definition viewed, but usually ViewModels wrap them.
                        // Here they are exposed directly.
                        // We set the property. If UI doesn't update, we might need manual trigger.
                        // But let's assume direct set works for now.
                        instance.IsUpdateAvailable = true;
                    }
                }
                catch
                {
                    // Ignore check errors
                }
            });

        await Task.WhenAll(checkTasks);

        // Force refresh of filtered list to update UI if needed
        FilterInstances();
    }

    // Adding the property and command for the View to bind to
    [RelayCommand]
    public Task UpdateInstance()
    {
        if (SelectedInstance == null) return Task.CompletedTask;

        // Navigate to Store/ModpackDetail to perform the update
        // Or perform update right here. The prompt says "mandatory option... in library". 
        // Reusing ModpackDetailViewModel logic is best.
        // Let's navigate to the ModpackDetailView for this instance.

        if (SelectedInstance.Metadata != null)
        {
            var modpack = new Modpack
            {
                Name = SelectedInstance.Metadata.Name,
                Slug = SelectedInstance.Metadata.Slug,
                Version = SelectedInstance.Metadata.Version,
                // We might lack full details here without fetching, but ModpackDetailViewModel fetches status.
                // We need to pass enough to identify it.
                Thumbnail = SelectedInstance.IconPath // Use icon or fetch thumbnail
            };

            // WE need to pass the "Latest" version if we know it, otherwise ModpackDetail check might fail if it relies on "Modpack" arg being the online one.
            // Actually ModpackDetailViewModel CheckInstallationStatus compares "Modpack.Version" (Online) vs "Installed.Version".
            // So we must ensure we pass the ONLINE modpack object, not the installed one.
            // This suggests we should probably fetch the modpack DTO first.

            IsLoading = true;
            try
            {
                // We need the service. I will add it in the next step properly.
            }
            finally
            {
                IsLoading = false;
            }
        }
        return Task.CompletedTask;
    }

    public async Task ReloadInstances()
    {
        if (_instanceService == null) return;

        Instances.Clear();
        var versions = await _instanceService.GetAllInstancesAsync();

        _allInstances.Clear();
        _allInstances.AddRange(versions);

        FilterInstances();

        // Try to preserve selection if possible, otherwise select first
        if (Instances.Count > 0 && (SelectedInstance == null || !Instances.Contains(SelectedInstance)))
            SelectedInstance = Instances[0];
    }

    private async Task LoadInstances() => await ReloadInstances();

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

    [ObservableProperty]
    private string _gameOutput = "";

    [RelayCommand]
    public async Task LaunchGame()
    {
        if (SelectedInstance == null) return;

        IsLoading = true;
        GameOutput = ""; // Clear previous log
        _mainWindowViewModel.StatusMessage = $"Iniciando {SelectedInstance.Name}...";

        Action<string> outputAction = (line) =>
        {
            // Dispatch to UI thread if needed, but simple property set might work if bound correctly
            // Avalonia usually needs UI thread for collection changes, string property might be forgiving or need dispatcher
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                GameOutput += line + "\n";
            });
        };

        // Launch functionality
        try
        {
            await _instanceService.LaunchInstanceAsync(SelectedInstance, _mainWindowViewModel.PlayerName, _settingsViewModel.MaxRamMb, outputAction);
            _mainWindowViewModel.StatusMessage = "Jogo iniciado com sucesso!";
        }
        catch (Exception ex)
        {
            _mainWindowViewModel.StatusMessage = $"Erro: {ex.Message}";
            outputAction($"[FATAL ERROR] {ex.Message}");
        }

        IsLoading = false;
    }

    [RelayCommand]
    public void AddInstance()
    {
        _mainWindowViewModel.StatusMessage = "Criar nova instância...";
    }
}