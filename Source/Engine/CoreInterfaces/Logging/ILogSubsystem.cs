namespace Duck.Logging;

public interface ILogSubsystem : IApplicationSubsystem
{
    public ILogger CreateLogger(string categoryName);
}
