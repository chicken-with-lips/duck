using System;
using System.Diagnostics;
using Duck;
using Duck.GameFramework;
using Duck.Input;
using Duck.Logging;
using Silk.NET.Input;

namespace Editor.Host;

public class EditorClientHostModule : ITickableModule
{
    private readonly ILogger _logger;
    private readonly EditorClientHost _clientHost;

    public EditorClientHostModule(ApplicationBase application, ILogModule logModule)
    {
        _logger = logModule.CreateLogger("GameHost");
        _logger.LogInformation("Initializing game host module.");

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
