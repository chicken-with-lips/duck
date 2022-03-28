using System;
using Duck;
using Duck.Logging;

namespace Editor.Host;

public class EditorClientHostSubsystem : IApplicationTickableSubsystem
{
    private readonly ILogger _logger;
    private readonly EditorClientHost _clientHost;

    public EditorClientHostSubsystem(IApplication application, ILogSubsystem logSubsystem)
    {
        _logger = logSubsystem.CreateLogger("GameHost");
        _logger.LogInformation("Initializing game host subsystem.");

        _clientHost = new EditorClientHost(application, _logger);

        if (!_clientHost.LoadAndInitialize()) {
            throw new Exception("Failed to initialize game client");
        }
    }

    public void Tick()
    {
        _clientHost.Tick();
    }
}
