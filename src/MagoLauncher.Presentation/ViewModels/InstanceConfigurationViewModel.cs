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
    private readonly Action? _onDeleted;

    public InstanceConfigurationViewModel(MinecraftInstance instance, IMinecraftInstanceService instanceService, Action onClose, Action? onDeleted = null)
    {
        _instance = instance;
        _instanceService = instanceService;
        _onClose = onClose;
        _onDeleted = onDeleted;
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

    [RelayCommand]
    public void OpenFolder()
    {
        if (_instance == null) return;

        var path = _instance.InstancePath;
        if (string.IsNullOrEmpty(path))
        {
            path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "instances", _instance.Name);
        }

        // Ensure directory exists
        if (!System.IO.Directory.Exists(path))
        {
            // Fallback to instances root
            path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "instances");
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch { /* Ignore explorer launch errors */ }
    }

    [RelayCommand]
    public async Task Verify()
    {
        // Placeholder for verification logic
        await Task.Delay(500); // Simulate work
    }

    [ObservableProperty]
    private bool _isConfirmingDelete;

    [RelayCommand]
    public void RequestDelete()
    {
        IsConfirmingDelete = true;
    }

    [RelayCommand]
    public void CancelDelete()
    {
        IsConfirmingDelete = false;
    }

    [RelayCommand]
    public async Task Delete()
    {
        if (!IsConfirmingDelete)
        {
            IsConfirmingDelete = true;
            return;
        }

        try
        {
            await _instanceService.DeleteInstanceAsync(_instance);

            // Invoke callbacks on UI thread if necessary, but Action usually okay.
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _onClose?.Invoke();
                _onDeleted?.Invoke();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting instance: {ex.Message}");
            // In a real app, we'd show a dialog here.
        }
    }
}
