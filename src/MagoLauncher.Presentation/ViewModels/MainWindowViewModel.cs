using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Application.Services;
using MagoLauncher.Presentation.Services;
using MagoLauncher.Presentation.Services.Navigation;
using System.Threading.Tasks;

namespace MagoLauncher.Presentation.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "MagoLauncher";

    [ObservableProperty]
    private string _statusMessage = "Pronto para jogar";

    [ObservableProperty]
    private ViewModelBase _currentPage = null!;

    [ObservableProperty]
    private string _playerName = "MagoPlayer";

    [ObservableProperty]
    private string _activeView = "Home";

    private readonly INavigationService _navigationService;
    private readonly ISettingsService _settingsService;
    private readonly IDebugConsoleService _debugConsoleService;

    // Design-time constructor
    public MainWindowViewModel()
    {
        _settingsService = null!;
        _debugConsoleService = null!;
        _navigationService = null!;
        _currentPage = null!;
    }

    public MainWindowViewModel(
        ISettingsService settingsService,
        IDebugConsoleService debugConsoleService,
        INavigationService navigationService,
        IStatusService statusService)
    {
        _settingsService = settingsService;
        _debugConsoleService = debugConsoleService;
        _navigationService = navigationService;

        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

        // Subscribe to status updates
        statusService.StatusMessageChanged += (msg) => StatusMessage = msg;

        InitializeSettings();

        // Initial navigation
        // We defer this slightly or just call it. 
        // Note: HomeViewModel creation will now be handled by DI when we navigate.
        _navigationService.NavigateTo<HomeViewModel>();
    }

    private void OnCurrentViewModelChanged(ViewModelBase viewModel)
    {
        CurrentPage = viewModel;

        // Update ActiveView for sidebar highlighting based on VM type
        if (viewModel is HomeViewModel) ActiveView = "Home";
        else if (viewModel is StoreViewModel || viewModel is ModpackDetailViewModel) ActiveView = "Store";
        else if (viewModel is SettingsViewModel) ActiveView = "Settings";
        else ActiveView = "";
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

    [RelayCommand]
    public void GoToHome()
    {
        _navigationService.NavigateTo<HomeViewModel>();
    }

    [RelayCommand]
    public void GoToSettings()
    {
        _navigationService.NavigateTo<SettingsViewModel>();
    }

    [RelayCommand]
    public void GoToStore()
    {
        _navigationService.NavigateTo<StoreViewModel>();
    }
}