namespace Duck.Logging;

public interface ILogger
{
    public void LogDebug(string message, params object[] args);
    public void LogInformation(string message, params object[] args);
    public void LogError(string message, params object[] args);
    public void LogWarning(string message, params object[] args);
}
