using System.Collections.Concurrent;
using Duck.Logging.Providers;
using Microsoft.Extensions.Logging;
using ProviderLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Duck.Logging;

public class LogModule : ILogModule
{
    #region Members

    private readonly ILoggerFactory _factory;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, ILogger> _loggers = new();

    #endregion

    #region Methods

    public LogModule()
    {
        _factory = LoggerFactory.Create(builder => {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddProvider(new ConsoleLoggerProvider(LogLevel.Debug))
                .AddProvider(new ConsoleLoggerProvider(LogLevel.Error, ConsoleColor.Black, ConsoleColor.Red))
                .AddProvider(new ConsoleLoggerProvider(LogLevel.Information))
                .AddProvider(new ConsoleLoggerProvider(LogLevel.Warning, ConsoleColor.Red));
        });

        _logger = CreateLogger("Log");
        _logger.LogInformation("Created logging module.");
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new Logger(_factory.CreateLogger(name)));
    }

    #endregion
}
