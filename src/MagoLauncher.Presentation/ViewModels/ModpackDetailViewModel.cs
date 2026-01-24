using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MagoLauncher.Presentation.Models;
using System.Threading.Tasks;

namespace MagoLauncher.Presentation.ViewModels;

public partial class ModpackDetailViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    [ObservableProperty]
    private Modpack _modpack;

    public ModpackDetailViewModel(MainWindowViewModel mainWindowViewModel, Modpack modpack)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _modpack = modpack;
    }

    // Default constructor for design preview
    public ModpackDetailViewModel()
    {
        _mainWindowViewModel = new MainWindowViewModel();
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
    public async Task Install()
    {
        // TODO: Implement install logic
        await Task.CompletedTask;
    }
}
