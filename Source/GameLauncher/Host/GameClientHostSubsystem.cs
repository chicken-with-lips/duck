using System;
using Duck;
using Duck.Logging;

namespace Game.Host;

public class GameClientHostModule : ITickableModule
{
    private readonly ILogger _logger;
    private readonly GameClientHost _clientHost;

    public GameClientHostModule(IApplication application, ILogModule logModule)
    {
        _logger = logModule.CreateLogger("GameHost");
        _logger.LogInformation("Initializing client host module.");

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
