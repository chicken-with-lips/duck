using Microsoft.Extensions.Logging;
using ProviderLogger = Microsoft.Extensions.Logging.ILogger;

namespace Duck.Logging;

public class Logger : ILogger
{
    #region Members

    private readonly ProviderLogger _logger;

    #endregion

    #region Methods

    internal Logger(ProviderLogger logger)
    {
        _logger = logger;
    }

    #endregion

    #region ILogger

    public void LogDebug(string message, params object[] args)
    {
        Console.WriteLine(message, args);
        // _logger.LogDebug(message, args);
    }

    public void LogInformation(string message, params object[] args)
    {
        Console.WriteLine(message, args);
        // _logger.LogInformation(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        Console.WriteLine(message, args);
        // _logger.LogError(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        Console.WriteLine(message, args);
        // _logger.LogWarning(message, args);
    }

    #endregion
}
