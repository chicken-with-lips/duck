using Microsoft.Extensions.Logging;
using ProviderLogger = Microsoft.Extensions.Logging.ILogger;

namespace Duck.Platform.Logging;

public class Logger
{
    #region Members

    private readonly ProviderLogger _logger;

    #endregion

    #region Methods

    internal Logger(ProviderLogger logger)
    {
        _logger = logger;
    }

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