using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace MagoLauncher.Presentation.Services;

public class LogService
{
    public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

    public void Log(string message)
    {
        // Ensure UI updates happen on the UI thread
        Dispatcher.UIThread.Post(() =>
        {
            var timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Logs.Add(timestampedMessage);
            System.Diagnostics.Debug.WriteLine(timestampedMessage); // Keep debug output as well
        });
    }

    public void Error(string message, Exception? ex = null)
    {
        Log($"ERROR: {message} {ex}");
    }
}
