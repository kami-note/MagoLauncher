using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Application.Services;
using MagoLauncher.Presentation.Models;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace MagoLauncher.Presentation.ViewModels;

public partial class ModpackDetailViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly IModpackService _modpackService;
    private readonly INotificationService _notificationService;
    private readonly IMinecraftInstanceService _instanceService;

    [ObservableProperty]
    private Modpack _modpack;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotInstalled))]
    private bool _isInstalled;

    public bool IsNotInstalled => !IsInstalled;

    public ModpackDetailViewModel(MainWindowViewModel mainWindowViewModel, Modpack modpack, IModpackService modpackService, INotificationService notificationService, IMinecraftInstanceService instanceService)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _modpack = modpack;
        _modpackService = modpackService;
        _notificationService = notificationService;
        _instanceService = instanceService;

        IsInstalled = modpack.IsInstalled;

        // If passed modpack status was stale, double check (optional, but good for direct deep linking)
        _ = CheckInstallationStatus();
    }

    private async Task CheckInstallationStatus()
    {
        if (_instanceService == null) return;
        var instances = await _instanceService.GetAllInstancesAsync();

        var installedInstance = instances.FirstOrDefault(i =>
            string.Equals(i.Metadata?.Slug, Modpack.Slug, StringComparison.OrdinalIgnoreCase));

        if (installedInstance != null)
        {
            IsInstalled = true;
            Modpack.IsInstalled = true;
            Modpack.InstanceId = installedInstance.Id; // This is a Guid?

            // Check for update
            if (!string.Equals(installedInstance.Metadata?.Version, Modpack.Version, StringComparison.OrdinalIgnoreCase))
            {
                IsUpdateAvailable = true;
            }
        }
    }

    // Default constructor for design preview
    public ModpackDetailViewModel()
    {
        _mainWindowViewModel = new MainWindowViewModel();
        _modpackService = null!;
        _notificationService = null!;
        _instanceService = null!;
        _modpack = new Modpack
        {
            Name = "Modpack Exemplo",
            Author = "MagoDev",
            Description = "Este é um modpack de exemplo para visualização em tempo de design.",
            MinecraftVersion = "1.20.1",
            Version = "1.0.0",
            Downloads = 1500000
        };
    }

    [RelayCommand]
    public void GoBack()
    {
        _mainWindowViewModel.GoToStore();
    }

    [RelayCommand]
    public async Task Play()
    {
        if (Modpack.InstanceId.HasValue)
        {
            await _mainWindowViewModel.GoToHome();
        }
    }

    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [RelayCommand]
    public async Task Update()
    {
        if (_modpackService == null || Modpack == null || !Modpack.InstanceId.HasValue) return;
        if (IsDownloading) return;

        IsDownloading = true;
        DownloadProgress = 0;

        var dto = new MagoLauncher.Application.DTOs.ModpackApiDto
        {
            Name = Modpack.Name ?? "Unknown",
            Slug = Modpack.Slug ?? "unknown",
            Summary = Modpack.Summary,
            Description = Modpack.Description,
            Version = Modpack.Version ?? "0.0.0",
            MinecraftVersion = Modpack.MinecraftVersion ?? "1.0",
            Author = Modpack.Author,
            Thumbnail = Modpack.Thumbnail,
            DownloadLink = Modpack.DownloadLink ?? "",
            Changelogs = Modpack.Changelogs
        };

        try
        {
            var progress = new System.Progress<double>(percent =>
            {
                DownloadProgress = percent;
            });

            // InstanceId is Guid?, UpdateModpackAsync expects string instanceId (which is likely just the ID)
            await _modpackService.UpdateModpackAsync(dto, Modpack.InstanceId.Value.ToString(), progress);

            _notificationService?.ShowSuccess("Sucesso", "Modpack atualizado com sucesso!");

            IsUpdateAvailable = false;
            Modpack.IsInstalled = true;
            IsInstalled = true;
        }
        catch (System.Exception ex)
        {
            _notificationService?.ShowError("Erro na Atualização", $"Falha ao atualizar modpack: {ex.Message}");
        }
        finally
        {
            IsDownloading = false;
        }
    }

    [RelayCommand]
    public async Task Install()
    {
        if (IsInstalled)
        {
            if (IsUpdateAvailable)
            {
                await Update();
            }
            else
            {
                await Play();
            }
            return;
        }

        if (_modpackService == null || Modpack == null) return;
        if (IsDownloading) return;

        IsDownloading = true;
        DownloadProgress = 0;

        var dto = new MagoLauncher.Application.DTOs.ModpackApiDto
        {
            Name = Modpack.Name ?? "Unknown",
            Slug = Modpack.Slug ?? "unknown",
            Summary = Modpack.Summary,
            Description = Modpack.Description,
            Version = Modpack.Version ?? "0.0.0",
            MinecraftVersion = Modpack.MinecraftVersion ?? "1.0",
            Author = Modpack.Author,
            Thumbnail = Modpack.Thumbnail,
            DownloadLink = Modpack.DownloadLink ?? "",
            Changelogs = Modpack.Changelogs
        };

        try
        {
            var progress = new System.Progress<double>(percent =>
            {
                DownloadProgress = percent;
            });

            await _modpackService.InstallModpackAsync(dto, progress);

            await _mainWindowViewModel.GoToHome();
        }
        catch (System.Exception ex)
        {
            _notificationService?.ShowError("Falha na Instalação", $"Ocorreu um erro ao instalar o modpack: {ex.Message}");
            IsDownloading = false;
            DownloadProgress = 0;
        }
        finally
        {
            IsDownloading = false;
        }
    }
}
