using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using MagoLauncher.Presentation.ViewModels;
using MagoLauncher.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;
using MagoLauncher.Application.Services;
using MagoLauncher.Infrastructure.Services;
using MagoLauncher.Presentation.Services;
using MagoLauncher.Presentation.Services.Navigation;

namespace MagoLauncher.Presentation;

public partial class App : Avalonia.Application
{
    public IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations
            DisableAvaloniaDataAnnotationValidation();

            // Setup DI
            var services = ConfigureServices();
            ServiceProvider = services.BuildServiceProvider();

            // Resolve Main Window and ViewModel
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            var viewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            mainWindow.DataContext = viewModel;

            // Set Notification Host
            var notificationService = ServiceProvider.GetRequiredService<INotificationService>();
            if (notificationService is NotificationService ns)
            {
                ns.SetHost(mainWindow);
            }

            // Start Update Service
            var updateService = ServiceProvider.GetRequiredService<UpdateService>();
            _ = System.Threading.Tasks.Task.Run(async () => await updateService.CheckAndApplyUpdatesAsync());

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // Core Services
        services.AddSingleton<System.Net.Http.HttpClient>();

        // Application/Infrastructure Services
        services.AddSingleton<IModpackService, ModpackService>();
        services.AddSingleton<IMinecraftInstanceService, MinecraftInstanceService>();
        services.AddSingleton<IGameSessionService, GameSessionService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        // Presentation Services
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<LogService>();
        services.AddSingleton<IDebugConsoleService, DebugConsoleService>();
        services.AddSingleton<UpdateService>();
        services.AddSingleton<IStatusService, StatusService>();

        // Navigation
        services.AddSingleton<INavigationService, NavigationService>();

        // Views
        services.AddSingleton<MainWindow>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();

        // Child ViewModels (Singletons to preserve state)
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<StoreViewModel>();
        services.AddSingleton<SettingsViewModel>();

        return services;
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}