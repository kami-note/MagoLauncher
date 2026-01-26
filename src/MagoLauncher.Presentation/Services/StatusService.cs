using System;

namespace MagoLauncher.Presentation.Services;

public interface IStatusService
{
    string StatusMessage { get; set; }
    event Action<string> StatusMessageChanged;
}

public class StatusService : IStatusService
{
    private string _statusMessage = "Pronto";

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                StatusMessageChanged?.Invoke(_statusMessage);
            }
        }
    }

    public event Action<string>? StatusMessageChanged;
}
