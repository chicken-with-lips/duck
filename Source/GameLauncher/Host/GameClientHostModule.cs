using System;
using System.Runtime.CompilerServices;
using Duck;
using Duck.Logging;

namespace GameLauncher.Host;

public class GameClientHostModule : IInitializableModule, ITickableModule, IShutdownModule
{
    private readonly IApplication _app;
    private readonly string _projectDirectory;
    private readonly ILogger _logger;
    private GameClientHost? _clientHost;

    public GameClientHostModule(IApplication app, ILogModule logModule, string projectDirectory)
    {
        _app = app;
        _projectDirectory = projectDirectory;
        _logger = logModule.CreateLogger("GameHost");
        
        _logger.LogInformation("Created client host module.");
    }

    public bool Init()
    {
        _logger.LogInformation("Initializing client host module...");

        _clientHost = new GameClientHost(_app, _logger, _projectDirectory);

        if (!_clientHost.LoadAndInitialize()) {
            throw new Exception("Failed to initialize client");
        }

        return true;
    }

    public void Tick()
    {
        _clientHost?.Tick();
    }

    public void Shutdown()
    {
        _clientHost?.ExitPlayMode();
    }
}
