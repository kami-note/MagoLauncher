using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Application.Services;
using MagoLauncher.Presentation.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using MagoLauncher.Presentation.Services.Navigation;
using MagoLauncher.Presentation.Services;

namespace MagoLauncher.Presentation.ViewModels;

public partial class ModpackDetailViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IStatusService _statusService;
    private readonly IModpackService _modpackService;
    private readonly INotificationService _notificationService;
    private readonly IMinecraftInstanceService _instanceService;

    [ObservableProperty]
    private Modpack _modpack;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotInstalled))]
    private bool _isInstalled;

    public bool IsNotInstalled => !IsInstalled;

    public ModpackDetailViewModel(
        Modpack modpack,
        IModpackService modpackService,
        INotificationService notificationService,
        IMinecraftInstanceService instanceService,
        INavigationService navigationService,
        IStatusService statusService)
    {
        _modpack = modpack;
        _modpackService = modpackService;
        _notificationService = notificationService;
        _instanceService = instanceService;
        _navigationService = navigationService;
        _statusService = statusService;

        IsInstalled = modpack.IsInstalled;

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
            Modpack.InstanceId = installedInstance.Id;

            // Check for update
            if (CheckForUpdate(installedInstance.Metadata?.Version, Modpack.Version))
            {
                IsUpdateAvailable = true;
            }
        }
    }

    private bool CheckForUpdate(string? localVersion, string? remoteVersion)
    {
        if (string.IsNullOrWhiteSpace(remoteVersion)) return false;
        if (string.IsNullOrWhiteSpace(localVersion)) return true;

        string Sanitize(string v)
        {
            var s = v.Trim();
            if (s.StartsWith("v", StringComparison.OrdinalIgnoreCase)) s = s.Substring(1);
            return s;
        }

        var sLocal = Sanitize(localVersion);
        var sRemote = Sanitize(remoteVersion);

        bool localParsed = Version.TryParse(sLocal, out Version? vLocal);
        bool remoteParsed = Version.TryParse(sRemote, out Version? vRemote);

        if (localParsed && remoteParsed)
        {
            if (vRemote > vLocal) return true;
            if (vRemote < vLocal) return false;
            return false;
        }

        if (string.Equals(sLocal, sRemote, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    // Default constructor for design preview
    public ModpackDetailViewModel()
    {
        _navigationService = null!;
        _statusService = null!;
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
        _navigationService.NavigateTo<StoreViewModel>();
    }

    [RelayCommand]
    public void Play()
    {
        if (Modpack.InstanceId.HasValue)
        {
            _navigationService.NavigateTo<HomeViewModel>();
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
        if (_modpackService == null || Modpack == null || !Modpack.InstanceId.HasValue)
        {
            _notificationService?.ShowError("Erro", "Não foi possível iniciar a atualização: Identificador da instância inválido.");
            return;
        }
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
                Play();
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

            _navigationService.NavigateTo<HomeViewModel>();
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
