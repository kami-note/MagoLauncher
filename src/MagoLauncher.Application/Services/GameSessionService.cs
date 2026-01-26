using MagoLauncher.Domain.Entities;
using System.Diagnostics;

namespace MagoLauncher.Application.Services;

public class GameSessionService : IGameSessionService
{
    private readonly IMinecraftInstanceService _instanceService;
    private readonly Dictionary<Guid, Process> _activeSessions = new();
    private readonly Dictionary<Guid, DateTime> _startTimes = new();

    public event EventHandler<SessionStartedEventArgs>? SessionStarted;
    public event EventHandler<SessionEndedEventArgs>? SessionEnded;

    public GameSessionService(IMinecraftInstanceService instanceService)
    {
        _instanceService = instanceService;
    }

    public void StartSession(MinecraftInstance instance, Process process)
    {
        if (_activeSessions.ContainsKey(instance.Id))
        {
            // Already running? Update process if it's new (unlikely) or just ignore
            return;
        }

        _activeSessions[instance.Id] = process;
        _startTimes[instance.Id] = DateTime.UtcNow;

        process.EnableRaisingEvents = true;
        process.Exited += (s, e) => HandleProcessExit(instance);

        SessionStarted?.Invoke(this, new SessionStartedEventArgs(instance.Id, process));
    }

    public bool IsGameRunning(Guid instanceId)
    {
        if (_activeSessions.TryGetValue(instanceId, out var process))
        {
            if (!process.HasExited) return true;
        }
        return false;
    }

    private void HandleProcessExit(MinecraftInstance instance)
    {
        if (!_startTimes.TryGetValue(instance.Id, out var startTime)) return;

        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Update stats locally
        instance.PlayTimeMinutes += (long)duration.TotalMinutes;
        instance.LastPlayedAt = endTime;

        // Persist
        _ = _instanceService.UpdateInstanceAsync(instance);

        // Cleanup
        _activeSessions.Remove(instance.Id);
        _startTimes.Remove(instance.Id);

        // Notify
        SessionEnded?.Invoke(this, new SessionEndedEventArgs(instance.Id, duration));
    }
}
