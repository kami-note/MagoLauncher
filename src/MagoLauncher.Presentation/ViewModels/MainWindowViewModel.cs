using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Domain.Entities;
using MagoLauncher.Domain.Enums;

namespace MagoLauncher.Presentation.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "MagoLauncher";

    [ObservableProperty]
    private string _statusMessage = "Pronto para jogar";

    [ObservableProperty]
    private MinecraftInstance? _selectedInstance;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<MinecraftInstance> Instances { get; } = [];

    public MainWindowViewModel()
    {
        // Adiciona instância de exemplo para demonstração
        Instances.Add(new MinecraftInstance
        {
            Name = "Minecraft 1.20.4",
            MinecraftVersion = "1.20.4",
            ModLoaderType = ModLoaderType.Vanilla
        });

        Instances.Add(new MinecraftInstance
        {
            Name = "Modded 1.20.1",
            MinecraftVersion = "1.20.1",
            ModLoaderType = ModLoaderType.Fabric,
            ModLoaderVersion = "0.15.0"
        });

        if (Instances.Count > 0)
            SelectedInstance = Instances[0];
    }

    [RelayCommand]
    private async Task LaunchGame()
    {
        if (SelectedInstance == null) return;

        IsLoading = true;
        StatusMessage = $"Iniciando {SelectedInstance.Name}...";

        // Simulação de lançamento
        await Task.Delay(1500);

        StatusMessage = "Jogo iniciado!";
        IsLoading = false;
    }

    [RelayCommand]
    private void AddInstance()
    {
        StatusMessage = "Criar nova instância...";
    }
}
