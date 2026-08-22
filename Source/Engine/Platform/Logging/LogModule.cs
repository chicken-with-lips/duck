using System.Collections.Concurrent;
using Duck.Platform.ModuleManagement;
using Microsoft.Extensions.Logging;

namespace Duck.Platform.Logging;

public class LogModule : IModule
{
    #region Members

    private readonly ILoggerFactory _factory;
    private readonly Logger _logger;
    private readonly ConcurrentDictionary<string, Logger> _loggers = new();

    #endregion

    #region Methods

    public LogModule()
    {
        _factory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddProvider(new ConsoleLoggerProvider(LogLevel.Debug))
                    .AddProvider(new ConsoleLoggerProvider(LogLevel.Error, ConsoleColor.Black, ConsoleColor.Red))
                    .AddProvider(new ConsoleLoggerProvider(LogLevel.Information))
                    .AddProvider(new ConsoleLoggerProvider(LogLevel.Warning, ConsoleColor.Red));
            }
        );

        _logger = CreateLogger("Log");
        _logger.LogInformation("Created logging module.");
    }

    public Logger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new Logger(_factory.CreateLogger(name)));
    }

    #endregion
}
