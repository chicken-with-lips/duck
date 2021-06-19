using System;
using Duck.Contracts;
using Duck.Contracts.Logging;

namespace Launcher.ClientHost
{
    public class ClientHostSubsystem : IApplicationInitializableSubsystem, IApplicationTickableSubsystem
    {
        private readonly IApplication _application;
        private readonly ILogSubsystem _logSubsystem;

        private bool _isInitialized;
        private ILogger? _logger;
        private ClientHost? _clientHost;

        public ClientHostSubsystem(IApplication application, ILogSubsystem logSubsystem)
        {
            _application = application;
            _logSubsystem = logSubsystem;
        }

        public bool Init()
        {
            if (_isInitialized) {
                throw new Exception("ClientHostSubsystem has already been initialized");
            }

            _logger = _logSubsystem.CreateLogger("ClientHost");
            _logger.LogInformation("Initializing client host subsystem.");

            _clientHost = new ClientHost(_application, _logger);

            if (!_clientHost.LoadAndInitialize()) {
                return false;
            }

            _isInitialized = true;

            return true;
        }

        public void Tick()
        {
            _clientHost?.Tick();
        }
    }
}
