using CommunityToolkit.Mvvm.ComponentModel;
using MagoLauncher.Domain.Entities;
using System.Collections.ObjectModel;

namespace MagoLauncher.Presentation.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [ObservableProperty]
    private MinecraftInstance? _selectedInstance;

    public ObservableCollection<MinecraftInstance> Instances { get; }

    public HomeViewModel(ObservableCollection<MinecraftInstance> instances)
    {
        Instances = instances;
        if (Instances.Count > 0)
            SelectedInstance = Instances[0];
    }
}
