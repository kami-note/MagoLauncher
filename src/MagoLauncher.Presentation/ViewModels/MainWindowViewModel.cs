using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Application.Services;
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

    public MainWindowViewModel(IMinecraftInstanceService instanceService, IModpackService modpackService, INotificationService notificationService)
    {
        _notificationService = notificationService;
        _modpackService = modpackService;
        _settingsPage = new SettingsViewModel();
        _storePage = new StoreViewModel(this, _notificationService);
        // Pass dependencies to HomeViewModel
        _homePage = new HomeViewModel(instanceService, _settingsPage, this);

        _currentPage = _homePage;
        _activeView = "Home"; // Initialize active view
    }

    public MainWindowViewModel()
    {
        // Constructor for design-time preview
        _modpackService = null!;
        _notificationService = null!;
        _settingsPage = new SettingsViewModel();
        _storePage = new StoreViewModel();
        _homePage = new HomeViewModel(null!, _settingsPage, this);
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
        CurrentPage = new ModpackDetailViewModel(this, modpack, _modpackService, _notificationService);
        ActiveView = "Store"; // Keep "Store" active in sidebar
    }
}