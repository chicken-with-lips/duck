using Microsoft.Extensions.Logging;
using CoreLogger = Microsoft.Extensions.Logging.ILogger;
using ILogger = Duck.Contracts.Logging.ILogger;

namespace Duck.Logging
{
    public class Logger : ILogger
    {
        #region Members

        private readonly CoreLogger _logger;

        #endregion

        #region Methods

        internal Logger(CoreLogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region ILogger

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _logger.LogError(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        #endregion
    }
}
