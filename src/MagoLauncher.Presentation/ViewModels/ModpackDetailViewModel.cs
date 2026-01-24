using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MagoLauncher.Application.Services;
using MagoLauncher.Presentation.Models;
using System.Threading.Tasks;

namespace MagoLauncher.Presentation.ViewModels;

public partial class ModpackDetailViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly IModpackService _modpackService;

    [ObservableProperty]
    private Modpack _modpack;

    public ModpackDetailViewModel(MainWindowViewModel mainWindowViewModel, Modpack modpack, IModpackService modpackService)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _modpack = modpack;
        _modpackService = modpackService;
    }

    // Default constructor for design preview
    public ModpackDetailViewModel()
    {
        _mainWindowViewModel = new MainWindowViewModel();
        _modpackService = null!;
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


    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private bool _isDownloading;

    [RelayCommand]
    public async Task Install()
    {
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

            // Navigate to home to show new instance
            await _mainWindowViewModel.GoToHome();
        }
        catch (System.Exception ex)
        {
            // TODO: Show error
            System.Console.WriteLine($"Install failed: {ex}");
            // Reset state on failure so user can try again
            IsDownloading = false;
            DownloadProgress = 0;
        }
        finally
        {
            IsDownloading = false;
        }
    }
}
