using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Application.Services;
using MagoLauncher.Presentation.Services;
using System.Threading.Tasks;

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
    private string _playerName = "MagoPlayer";

    [ObservableProperty]
    private string _activeView = "Home";

    private readonly HomeViewModel _homePage;
    private readonly SettingsViewModel _settingsPage;
    private readonly StoreViewModel _storePage;

    private readonly IModpackService _modpackService;
    private readonly INotificationService _notificationService;
    private readonly IMinecraftInstanceService _instanceService;
    private readonly ISettingsService _settingsService;
    private readonly IDebugConsoleService _debugConsoleService;

    public MainWindowViewModel(IMinecraftInstanceService instanceService, IModpackService modpackService, INotificationService notificationService, ISettingsService settingsService, IDebugConsoleService debugConsoleService)
    {
        _notificationService = notificationService;
        _modpackService = modpackService;
        _instanceService = instanceService;
        _settingsService = settingsService;
        _debugConsoleService = debugConsoleService;

        _settingsPage = new SettingsViewModel();
        _storePage = new StoreViewModel(this, _notificationService, _instanceService);
        // Pass dependencies to HomeViewModel
        _homePage = new HomeViewModel(instanceService, _settingsPage, this, _modpackService);


        _currentPage = _homePage;
        _activeView = "Home"; // Initialize active view

        InitializeSettings();
    }

    [RelayCommand]
    public void ToggleConsole()
    {
        _debugConsoleService.Toggle();
    }

    private async void InitializeSettings()
    {
        var settings = await _settingsService.LoadSettingsAsync();
        PlayerName = settings.PlayerName;
    }

    partial void OnPlayerNameChanged(string value)
    {
        _ = SavePlayerName(value);
    }

    private async Task SavePlayerName(string name)
    {
        var settings = await _settingsService.LoadSettingsAsync();
        settings.PlayerName = name;
        await _settingsService.SaveSettingsAsync(settings);
    }

    public MainWindowViewModel()
    {
        // Constructor for design-time preview
        _modpackService = null!;
        _notificationService = null!;
        _instanceService = null!;
        _settingsService = null!;
        _debugConsoleService = null!;
        _settingsPage = new SettingsViewModel();
        _storePage = new StoreViewModel();
        _homePage = new HomeViewModel(null!, _settingsPage, this, null!);
        _currentPage = _homePage;
        _activeView = "Home"; // Initialize active view
    }

    [RelayCommand]
    public async Task GoToHome()
    {
        CurrentPage = _homePage;
        ActiveView = "Home";
        await _homePage.ReloadInstances();
    }

    [RelayCommand]
    public void GoToSettings()
    {
        CurrentPage = _settingsPage;
        ActiveView = "Settings";
    }

    [RelayCommand]
    public void GoToStore()
    {
        CurrentPage = _storePage;
        ActiveView = "Store";
    }

    public void GoToModpackDetails(Models.Modpack modpack)
    {
        CurrentPage = new ModpackDetailViewModel(this, modpack, _modpackService, _notificationService, _instanceService);
        ActiveView = "Store"; // Keep "Store" active in sidebar
    }
}