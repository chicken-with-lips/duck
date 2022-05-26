using System;
using System.Diagnostics;
using Duck;
using Duck.GameFramework;
using Duck.Input;
using Duck.Logging;
using Silk.NET.Input;

namespace Editor.Host;

public class EditorClientHostModule : IInitializableModule, ITickableModule
{
    private readonly ILogger _logger;
    private readonly EditorClientHost _clientHost;

    public EditorClientHostModule(ApplicationBase application, ILogModule logModule)
    {
        _logger = logModule.CreateLogger("GameHost");
        _logger.LogInformation("Created game host module.");

        _clientHost = new EditorClientHost(application, _logger);
    }

    public bool Init()
    {
        _logger.LogInformation("Initializing game host module...");

        if (!_clientHost.LoadAndInitialize()) {
            throw new Exception("Failed to initialize game client");
        }

        return true;
    }

    public void Tick()
    {
        _clientHost.Tick();
    }
}
