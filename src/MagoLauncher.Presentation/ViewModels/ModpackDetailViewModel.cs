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
            // Check for update
            if (CheckForUpdate(installedInstance.Metadata?.Version, Modpack.Version))
            {
                IsUpdateAvailable = true;
            }
        }
    }

    private bool CheckForUpdate(string? localVersion, string? remoteVersion)
    {
        if (string.IsNullOrWhiteSpace(remoteVersion)) return false; // No remote version = no update
        if (string.IsNullOrWhiteSpace(localVersion)) return true;   // Check logic: if local has no version but remote does, assume update? 
                                                                    // Or assume broken install. Let's assume update to be safe/fix it.

        // Debug logging
        System.Diagnostics.Debug.WriteLine($"[UpdateCheck] Local: '{localVersion}' | Remote: '{remoteVersion}'");
        System.Console.WriteLine($"[UpdateCheck] Local: '{localVersion}' | Remote: '{remoteVersion}'");

        string Sanitize(string v)
        {
            var s = v.Trim();
            if (s.StartsWith("v", StringComparison.OrdinalIgnoreCase)) s = s.Substring(1);
            // Handle "-beta", "-release" etc by taking only the first part? 
            // System.Version supports major.minor.build.revision. 
            // If we have "1.0.1-beta", parsing fails. 
            // Let's try to split by '-' and take the first part for Version parsing, 
            // but this loses semantic pre-release info (1.0.1-beta < 1.0.1).
            // For now, let's just strip 'v'.
            return s;
        }

        var sLocal = Sanitize(localVersion);
        var sRemote = Sanitize(remoteVersion);

        // Try parse
        bool localParsed = Version.TryParse(sLocal, out Version? vLocal);
        bool remoteParsed = Version.TryParse(sRemote, out Version? vRemote);

        if (localParsed && remoteParsed)
        {
            // Robust comparison
            if (vRemote > vLocal)
            {
                System.Console.WriteLine("[UpdateCheck] Remote > Local -> Update Available");
                return true;
            }
            if (vRemote < vLocal)
            {
                System.Console.WriteLine("[UpdateCheck] Remote < Local -> No Update (Downgrade?)");
                return false;
            }
            // Equal
            return false;
        }

        // Fallback to string check if parsing fails (e.g. custom non-numeric versions)
        // But NOT simple inequality. If strings are equal, no update.
        if (string.Equals(sLocal, sRemote, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // If different and not parsable, default to notifying update?
        // Or checking if remote "looks" newer? Hard to say. 
        // Current behavior was: different = update.
        // Let's keep that for non-parsable strings to ensure users don't miss updates,
        // but now strict equality (case-insensitive) prevents false positives on "1.0" vs "1.0"

        // One edge case: "1.0" vs "1.0.0" -> if they fail parsing (unlikely), they are different strings.
        // But Version.TryParse handles "1.0" -> 1.0 (-1,-1). "1.0.0" -> 1.0.0 (-1).
        // Version comparison: 1.0 == 1.0.0. So parsing covers this.

        System.Console.WriteLine("[UpdateCheck] Non-parsable versions differ -> Update Available");
        return true;
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
        System.Diagnostics.Debug.WriteLine($"[UpdateCommand] Clicked. Modpack: {Modpack?.Name}, InstanceId: {Modpack?.InstanceId}");

        if (_modpackService == null || Modpack == null || !Modpack.InstanceId.HasValue)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateCommand] ABORTED. Service: {_modpackService != null}, Modpack: {Modpack != null}, InstanceId: {Modpack?.InstanceId}");
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
