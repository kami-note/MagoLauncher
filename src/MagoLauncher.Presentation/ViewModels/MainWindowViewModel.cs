using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Application.Services;

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

    public MainWindowViewModel(IMinecraftInstanceService instanceService)
    {
        _settingsPage = new SettingsViewModel();
        _storePage = new StoreViewModel();
        // Pass dependencies to HomeViewModel
        _homePage = new HomeViewModel(instanceService, _settingsPage, this);
        
        _currentPage = _homePage;
        _activeView = "Home"; // Initialize active view
    }

    public MainWindowViewModel()
    {
        // Constructor for design-time preview
        _settingsPage = new SettingsViewModel();
        _storePage = new StoreViewModel();
        _homePage = new HomeViewModel(null!, _settingsPage, this);
        _currentPage = _homePage;
        _activeView = "Home"; // Initialize active view
    }
    
    [RelayCommand]
    public void GoToHome()
    {
        CurrentPage = _homePage;
        ActiveView = "Home";
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
}