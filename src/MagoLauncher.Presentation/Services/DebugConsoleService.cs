using Avalonia.Controls;
using MagoLauncher.Presentation.ViewModels;
using MagoLauncher.Presentation.Views;
using System;

namespace MagoLauncher.Presentation.Services;

public class DebugConsoleService : IDebugConsoleService
{
    private readonly LogService _logService;
    private LogWindow? _logWindow; // Keep a reference to the window

    public DebugConsoleService(LogService logService)
    {
        _logService = logService;
    }

    private void EnsureWindowCreated()
    {
        if (_logWindow == null)
        {
            _logWindow = new LogWindow
            {
                DataContext = new LogViewModel(_logService)
            };

            // When user clicks 'X', don't actually close, just hide.
            _logWindow.Closing += (sender, e) =>
            {
                e.Cancel = true; // Cancel the real close
                _logWindow.Hide(); // Hide it instead
            };
        }
    }

    public void Show()
    {
        EnsureWindowCreated();
        _logWindow!.Show();
        _logWindow.Activate();
    }

    public void Hide()
    {
        if (_logWindow != null && _logWindow.IsVisible)
        {
            _logWindow.Hide();
        }
    }

    public void Toggle()
    {
        EnsureWindowCreated();

        if (_logWindow!.IsVisible)
        {
            _logWindow.Hide();
        }
        else
        {
            _logWindow.Show();
            _logWindow.Activate();
        }
    }
}
