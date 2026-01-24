using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Application.Services;
using MagoLauncher.Domain.Entities;
using System.Threading.Tasks;
using System;

namespace MagoLauncher.Presentation.ViewModels;

public partial class InstanceConfigurationViewModel : ViewModelBase
{
    private readonly IMinecraftInstanceService _instanceService;
    private readonly MinecraftInstance _instance;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private bool _overrideGlobalRam;

    [ObservableProperty]
    private int _maxRamMb;

    [ObservableProperty]
    private string _javaArgs;

    private readonly Action _onClose;

    public InstanceConfigurationViewModel(MinecraftInstance instance, IMinecraftInstanceService instanceService, Action onClose)
    {
        _instance = instance;
        _instanceService = instanceService;
        _onClose = onClose;
        _title = $"Configurações: {instance.Name}";

        // Initialize from metadata
        _overrideGlobalRam = instance.Metadata?.OverrideGlobalRam ?? false;
        _maxRamMb = instance.Metadata?.MaxRamMb ?? 4096; // Default to 4GB if not set
        _javaArgs = instance.Metadata?.JavaArgs ?? "";
    }

    [RelayCommand]
    public void Close()
    {
        _onClose?.Invoke();
    }

    [RelayCommand]
    public async Task Save()
    {
        // Update metadata
        if (_instance.Metadata == null)
        {
            // Create metadata if it doesn't exist (though it should for our modpacks)
            // In scanning verify, we might need to be careful, but for now assume it exists or we can't save nicely
            // If null, we can't really save "modpack" specific settings easily without upgrading it to a modpack
            // But let's assume we can attach metadata if missing?
            // For now, return if null to avoid crash, or handle creation.
            return;
        }

        _instance.Metadata.OverrideGlobalRam = OverrideGlobalRam;
        _instance.Metadata.MaxRamMb = MaxRamMb;
        _instance.Metadata.JavaArgs = JavaArgs;

        await _instanceService.UpdateInstanceAsync(_instance);

        // We can expose an event or just let the user close it.
    }

    [RelayCommand]
    public void ToggleOverride()
    {
        OverrideGlobalRam = !OverrideGlobalRam;
    }
}
