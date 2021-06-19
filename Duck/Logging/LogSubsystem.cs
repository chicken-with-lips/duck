using System;
using System.Collections.Concurrent;
using Duck.Contracts;
using Duck.Contracts.Logging;
using Duck.Logging.Providers;
using Microsoft.Extensions.Logging;
using ILogger = Duck.Contracts.Logging.ILogger;

namespace Duck.Logging
{
    public class LogSubsystem : ILogSubsystem, IApplicationInitializableSubsystem
    {
        #region Members

        private bool _isInitialized = false;

        private ILoggerFactory? _factory;
        private ILogger? _logger;

        private readonly ConcurrentDictionary<string, ILogger> _loggers = new();

        #endregion

        #region ILogSubsystem

        public bool Init()
        {
            if (_isInitialized) {
                throw new Exception("LogSubsystem has already been initialized");
            }

            _factory = LoggerFactory.Create(builder => {
                builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddProvider(new ConsoleLoggerProvider(LogLevel.Debug))
                    .AddProvider(new ConsoleLoggerProvider(LogLevel.Error, ConsoleColor.Black, ConsoleColor.Red))
                    .AddProvider(new ConsoleLoggerProvider(LogLevel.Information))
                    .AddProvider(new ConsoleLoggerProvider(LogLevel.Warning, ConsoleColor.Red));
            });

            _isInitialized = true;

            _logger = CreateLogger("Log");
            _logger.LogInformation("Initialized logging subsystem.");

            return true;
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (!_isInitialized) {
                throw new Exception("LogSubsystem has not yet been initialized");
            }

            return _loggers.GetOrAdd(categoryName, name => new Logger(_factory.CreateLogger(name)));
        }

        #endregion
    }
}
