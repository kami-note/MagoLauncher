using MagoLauncher.Domain.Entities;
using System;
using System.Diagnostics;

namespace MagoLauncher.Application.Services;

public class SessionStartedEventArgs : EventArgs
{
    public Guid InstanceId { get; }
    public Process Process { get; }

    public SessionStartedEventArgs(Guid instanceId, Process process)
    {
        InstanceId = instanceId;
        Process = process;
    }
}

public class SessionEndedEventArgs : EventArgs
{
    public Guid InstanceId { get; }
    public TimeSpan Duration { get; }

    public SessionEndedEventArgs(Guid instanceId, TimeSpan duration)
    {
        InstanceId = instanceId;
        Duration = duration;
    }
}

public interface IGameSessionService
{
    event EventHandler<SessionStartedEventArgs> SessionStarted;
    event EventHandler<SessionEndedEventArgs> SessionEnded;

    void StartSession(MinecraftInstance instance, Process process);
    bool IsGameRunning(Guid instanceId);
}
