using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ProviderLogger = Microsoft.Extensions.Logging.ILogger;

namespace Duck.Logging.Providers;

public class ConsoleLogger : ProviderLogger
{
    private readonly string _categoryName;
    private readonly LogLevel _logLevel;
    private readonly ConsoleColor _textColor;
    private readonly ConsoleColor _backgroundColor;

    public ConsoleLogger(string categoryName, LogLevel logLevel, ConsoleColor textColor, ConsoleColor backgroundColor)
    {
        _categoryName = categoryName;
        _logLevel = logLevel;
        _textColor = textColor;
        _backgroundColor = backgroundColor;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) {
            return;
        }

        if (_textColor != ConsoleColor.Black) {
            Console.ForegroundColor = _textColor;
        }

        if (_backgroundColor != ConsoleColor.Black) {
            Console.BackgroundColor = _backgroundColor;
        }

        Console.WriteLine("[{0:hh:mm:ss.fff}][{1}] {2}", DateTime.Now,
            _categoryName,
            formatter(state, exception)
        );

        Console.ResetColor();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == _logLevel;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }
}

public class ConsoleLoggerProvider : ILoggerProvider
{
    private readonly LogLevel _logLevel;
    private readonly ConsoleColor _textColor;
    private readonly ConsoleColor _backgroundColor;
    private readonly ConcurrentDictionary<string, ConsoleLogger> _loggers = new();

    public ConsoleLoggerProvider(LogLevel logLevel, ConsoleColor textColor = ConsoleColor.Black, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        _logLevel = logLevel;
        _textColor = textColor;
        _backgroundColor = backgroundColor;
    }

    public void Dispose()
    {
        _loggers.Clear();
    }

    public ProviderLogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new ConsoleLogger(name, _logLevel, _textColor, _backgroundColor));
    }
}
