using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using MagoLauncher.Presentation.ViewModels;
using MagoLauncher.Presentation.Views;

namespace MagoLauncher.Presentation;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var httpClient = new System.Net.Http.HttpClient();
            var modpackService = new MagoLauncher.Infrastructure.Services.ModpackService(httpClient);
            var instanceService = new MagoLauncher.Infrastructure.Services.MinecraftInstanceService();
            var settingsService = new MagoLauncher.Infrastructure.Services.SettingsService();
            var notificationService = new MagoLauncher.Presentation.Services.NotificationService();
            var logService = new MagoLauncher.Presentation.Services.LogService();

            var updateService = new MagoLauncher.Presentation.Services.UpdateService(logService);

            // Show developer log window
            var debugConsoleService = new MagoLauncher.Presentation.Services.DebugConsoleService(logService);

            // Check for updates in background
            _ = System.Threading.Tasks.Task.Run(async () => await updateService.CheckAndApplyUpdatesAsync());

            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(instanceService, modpackService, notificationService, settingsService, debugConsoleService),
            };

            notificationService.SetHost(mainWindow);
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void LogTrace(string message)
    {
        try
        {
            var path = @"C:\Users\yurin\MagoLaucher\startup_trace.txt";
            System.IO.File.AppendAllText(path, $"[{DateTime.Now}] {message}\n");
        }
        catch { }
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