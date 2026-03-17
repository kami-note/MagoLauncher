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
using MagoLauncher.Presentation.Services.Navigation;
using MagoLauncher.Presentation.Services;

namespace MagoLauncher.Presentation.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient = new();
    private readonly IMinecraftInstanceService _instanceService;
    private readonly IModpackService _modpackService;
    private readonly INavigationService _navigationService;
    private readonly ISettingsService _settingsService;
    private readonly IStatusService _statusService;
    private readonly IGameSessionService _gameSessionService;

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

    [ObservableProperty]
    private bool _isGameRunning;

    [ObservableProperty]
    private string _gameStatusText = "";

    [ObservableProperty]
    private string _playButtonText = "JOGAR";

    [ObservableProperty]
    private bool _isPlayButtonEnabled = true;

    partial void OnSelectedInstanceChanged(MinecraftInstance? value)
    {
        if (value == null) return;

        // Update Game Status
        if (_gameSessionService != null)
        {
            IsGameRunning = _gameSessionService.IsGameRunning(value.Id);
            GameStatusText = IsGameRunning ? "Rodando" : "";

            // Update Button State
            PlayButtonText = IsGameRunning ? "JOGANDO" : "JOGAR";
            IsPlayButtonEnabled = !IsGameRunning && !IsLoading;
        }

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

    public HomeViewModel(
        IMinecraftInstanceService instanceService,
        IModpackService modpackService,
        INavigationService navigationService,
        ISettingsService settingsService,
        IStatusService statusService,
        IGameSessionService gameSessionService)
    {
        _instanceService = instanceService;
        _modpackService = modpackService;
        _navigationService = navigationService;
        _settingsService = settingsService;
        _statusService = statusService;
        _gameSessionService = gameSessionService;

        _gameSessionService.SessionStarted += OnGameSessionStarted;
        _gameSessionService.SessionEnded += OnGameSessionEnded;

        Initialize();
    }

    private void OnGameSessionStarted(object? sender, SessionStartedEventArgs e)
    {
        if (SelectedInstance != null && SelectedInstance.Id == e.InstanceId)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsGameRunning = true;
                GameStatusText = "Rodando";
                PlayButtonText = "JOGANDO";
                IsPlayButtonEnabled = false;
            });
        }
    }

    private void OnGameSessionEnded(object? sender, SessionEndedEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (SelectedInstance != null && SelectedInstance.Id == e.InstanceId)
            {
                IsGameRunning = false;
                GameStatusText = "";
                PlayButtonText = "JOGAR";
                IsPlayButtonEnabled = !IsLoading;

                // Refresh specific stats if needed, or rely on ObservableProperty
                // Force notify properties that depend on stats
                OnPropertyChanged(nameof(LastPlayedText));
                OnPropertyChanged(nameof(PlayTimeText));
            }
        });
    }

    public HomeViewModel()
    {
        // Constructor for design-time preview
        _instanceService = null!;
        _modpackService = null!;
        _navigationService = null!;
        _settingsService = null!;
        _statusService = null!;
        _gameSessionService = null!;
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

                _navigationService.NavigateTo<ModpackDetailViewModel>(modpack);
            }
            catch (Exception ex)
            {
                // Fallback to local data if API fails, but notify
                System.Console.WriteLine($"[HomeViewModel] Failed to fetch update info: {ex}");
                _statusService.StatusMessage = $"Aviso: Falha ao obter dados remotos. Usando cache local.";

                var modpack = new Modpack
                {
                    Name = SelectedInstance.Metadata.Name,
                    Slug = SelectedInstance.Metadata.Slug,
                    Version = SelectedInstance.Metadata.Version,
                    Thumbnail = SelectedInstance.IconPath
                };
                modpack.IsInstalled = true;
                modpack.InstanceId = SelectedInstance.Id;

                _navigationService.NavigateTo<ModpackDetailViewModel>(modpack);
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
        _statusService.StatusMessage = $"Iniciando {SelectedInstance.Name}...";

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
            var settings = await _settingsService.LoadSettingsAsync();
            var playerName = settings.PlayerName ?? "MagoPlayer";
            var maxRam = settings.MaxRamMb; // Default 4096 if not set

            var process = await _instanceService.LaunchInstanceAsync(SelectedInstance, playerName, maxRam, outputAction);

            if (process != null)
            {
                _statusService.StatusMessage = "Jogo iniciado com sucesso!";
                _gameSessionService.StartSession(SelectedInstance, process);
            }
        }
        catch (Exception ex)
        {
            _statusService.StatusMessage = $"Erro: {ex.Message}";
            outputAction($"[FATAL ERROR] {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            // Re-evaluate button state
            if (SelectedInstance != null)
            {
                bool running = _gameSessionService.IsGameRunning(SelectedInstance.Id);
                IsPlayButtonEnabled = !running;
            }
        }
    }

    [RelayCommand]
    public void AddInstance()
    {
        _statusService.StatusMessage = "Criar nova instância...";
        // Navigate or Logic
    }
}