using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using MagoLauncher.Application.Services;
using System;

namespace MagoLauncher.Presentation.Services;

public class NotificationService : INotificationService
{
    private WindowNotificationManager? _notificationManager;

    public void SetHost(Window window)
    {
        _notificationManager = new WindowNotificationManager(window)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 3
        };
    }

    public void ShowSuccess(string title, string message)
    {
        _notificationManager?.Show(new Notification(title, message, NotificationType.Success));
    }

    public void ShowError(string title, string message)
    {
        _notificationManager?.Show(new Notification(title, message, NotificationType.Error));
    }

    public void ShowInformation(string title, string message)
    {
        _notificationManager?.Show(new Notification(title, message, NotificationType.Information));
    }

    public void ShowWarning(string title, string message)
    {
        _notificationManager?.Show(new Notification(title, message, NotificationType.Warning));
    }
}
