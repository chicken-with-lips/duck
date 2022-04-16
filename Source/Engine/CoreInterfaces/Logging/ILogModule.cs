namespace Duck.Logging;

public interface ILogModule : IModule
{
    public ILogger CreateLogger(string categoryName);
}
