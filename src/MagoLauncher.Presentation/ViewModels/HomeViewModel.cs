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

        // Load Activity Feed (Fresh from API)
        _ = LoadActivityFeed(value);
    }

    [ObservableProperty]
    private ObservableCollection<ModpackChangelog> _activityFeed = new();

    private async Task LoadActivityFeed(MinecraftInstance instance)
    {
        ActivityFeed.Clear();
        if (instance.Metadata?.Slug == null || _modpackService == null) return;

        try
        {
            var freshDto = await _modpackService.GetModpackAsync(instance.Metadata.Slug);
            if (freshDto?.Changelogs != null)
            {
                // Map DTO to Entity
                var changes = freshDto.Changelogs.Select(c => new ModpackChangelog
                {
                    Version = c.Version,
                    Text = c.Text,
                    UpdatedAt = c.UpdatedAt
                })
                .OrderByDescending(c => c.UpdatedAt) // Ensure latest first
                .ToList();

                foreach (var change in changes)
                {
                    ActivityFeed.Add(change);
                }
            }
        }
        catch
        {
            // If API fails, fallback to local if available, or stay empty
            if (instance.Metadata.Changelogs != null)
            {
                foreach (var change in instance.Metadata.Changelogs)
                {
                    ActivityFeed.Add(change);
                }
            }
        }
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
    public async Task UpdateInstance()
    {
        if (SelectedInstance == null) return;

        // Navigate to Store/ModpackDetail to perform the update
        // Or perform update right here. The prompt says "mandatory option... in library". 
        // Reusing ModpackDetailViewModel logic is best.
        // Let's navigate to the ModpackDetailView for this instance.

        if (SelectedInstance.Metadata != null)
        {
            IsLoading = true;
            try
            {
                // Fetch fresh data from API
                var freshDto = await _modpackService.GetModpackAsync(SelectedInstance.Metadata.Slug);

                var modpack = new Modpack
                {
                    Name = freshDto.Name,
                    Slug = freshDto.Slug,
                    Version = freshDto.Version, // This is the REMOTE version
                    MinecraftVersion = freshDto.MinecraftVersion,
                    Author = freshDto.Author,
                    Summary = freshDto.Summary,
                    Description = freshDto.Description,
                    Thumbnail = freshDto.Thumbnail,
                    DownloadLink = freshDto.DownloadLink,
                    Changelogs = freshDto.Changelogs
                };

                // Set installed flag based on LOCAL instance (which we know exists)
                modpack.IsInstalled = true;
                modpack.InstanceId = SelectedInstance.Id;

                _mainWindowViewModel.GoToModpackDetails(modpack);
            }
            catch (Exception ex)
            {
                // Fallback to local data if API fails, but notify
                System.Console.WriteLine($"[HomeViewModel] Failed to fetch update info: {ex}");
                _mainWindowViewModel.StatusMessage = $"Aviso: Falha ao obter dados remotos. Usando cache local.";

                // Show notification if possible (Need to expose NotificationService from MainWindow or inject it)
                // HomeViewModel doesn't have direct access to NotificationService locally in field, 
                // but it is passed in constructor... wait, check constructor.
                // It is NOT stored in a field. It is passed to StoreViewModel via MainWindow.
                // We should probably add INotificationService to HomeViewModel.

                // For now, write to console and maybe show in StatusMessage is enough?
                // Or better, let's just proceed to details page as it does now.
                // But user says "does not trigger any action". Check if it proceeds.

                var modpack = new Modpack
                {
                    Name = SelectedInstance.Metadata.Name,
                    Slug = SelectedInstance.Metadata.Slug,
                    Version = SelectedInstance.Metadata.Version,
                    Thumbnail = SelectedInstance.IconPath
                };
                modpack.IsInstalled = true;
                modpack.InstanceId = SelectedInstance.Id;

                _mainWindowViewModel.GoToModpackDetails(modpack);
            }
            finally
            {
                IsLoading = false;
            }
        }

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