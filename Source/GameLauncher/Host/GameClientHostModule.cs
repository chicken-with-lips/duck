using System;
using Duck;
using Duck.Logging;

namespace Game.Host;

public class GameClientHostModule : IInitializableModule, ITickableModule
{
    private readonly IApplication _app;
    private readonly ILogger _logger;
    private GameClientHost? _clientHost;

    public GameClientHostModule(IApplication app, ILogModule logModule)
    {
        _app = app;
        _logger = logModule.CreateLogger("GameHost");
    }

    public bool Init()
    {
        _logger.LogInformation("Initializing client host module.");

        _clientHost = new GameClientHost(_app, _logger);

        if (!_clientHost.LoadAndInitialize()) {
            throw new Exception("Failed to initialize client");
        }

        return true;
    }

    public void Tick()
    {
        _clientHost?.Tick();
    }
}
