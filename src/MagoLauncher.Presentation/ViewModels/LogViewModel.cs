using CommunityToolkit.Mvvm.ComponentModel;
using MagoLauncher.Presentation.Services;
using System.Collections.ObjectModel;

namespace MagoLauncher.Presentation.ViewModels;

public partial class LogViewModel : ObservableObject
{
    private readonly LogService _logService;

    public ObservableCollection<string> Logs => _logService.Logs;

    [ObservableProperty]
    private string _allLogs = string.Empty;

    public LogViewModel(LogService logService)
    {
        _logService = logService;
        _logService.Logs.CollectionChanged += Logs_CollectionChanged;
        UpdateAllLogs();
    }

    private void Logs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateAllLogs();
    }

    private void UpdateAllLogs()
    {
        AllLogs = string.Join(System.Environment.NewLine, _logService.Logs);
    }
}
