using System;
using Duck.Contracts;
using Duck.Contracts.Logging;

namespace Editor.ClientHost
{
    public class ClientHostSubsystem : IApplicationTickableSubsystem
    {
        private readonly ILogger _logger;
        private readonly ClientHost _clientHost;

        public ClientHostSubsystem(IApplication application, ILogSubsystem logSubsystem)
        {
            _logger = logSubsystem.CreateLogger("ClientHost");
            _logger.LogInformation("Initializing client host subsystem.");

            _clientHost = new ClientHost(application, _logger);

            if (!_clientHost.LoadAndInitialize()) {
                throw new Exception("Failed to initialize client");
            }
        }

        public void Tick()
        {
            _clientHost?.Tick();
        }
    }
}
