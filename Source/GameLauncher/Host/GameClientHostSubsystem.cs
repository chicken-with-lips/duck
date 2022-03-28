using System;
using Duck;
using Duck.Logging;

namespace Game.Host;

public class GameClientHostSubsystem : IApplicationTickableSubsystem
{
    private readonly ILogger _logger;
    private readonly GameClientHost _clientHost;

    public GameClientHostSubsystem(IApplication application, ILogSubsystem logSubsystem)
    {
        _logger = logSubsystem.CreateLogger("GameHost");
        _logger.LogInformation("Initializing client host subsystem.");

        _clientHost = new GameClientHost(application, _logger);

        if (!_clientHost.LoadAndInitialize()) {
            throw new Exception("Failed to initialize client");
        }
    }

    public void Tick()
    {
        _clientHost?.Tick();
    }
}
