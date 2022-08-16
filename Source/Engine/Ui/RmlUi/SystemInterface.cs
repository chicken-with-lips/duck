using ChickenWithLips.RmlUi;
using Duck.Logging;

namespace Duck.Ui.RmlUi;

internal class SystemInterface : ChickenWithLips.RmlUi.SystemInterface
{
    private readonly ILogger _logger;

    public SystemInterface(ILogger logger)
    {
        _logger = logger;
    }

    public override double ElapsedTime => Time.Elapsed;

    public override bool LogMessage(LogType type, string message)
    {
        switch (type) {
            case LogType.Error:
            case LogType.Assert:
                _logger.LogError(message);
                return false;
            case LogType.Warning:
                _logger.LogWarning(message);
                break;
            case LogType.Always:
            case LogType.Info:
                _logger.LogInformation(message);
                break;
            case LogType.Debug:
                _logger.LogDebug(message);
                break;
        }

        return true;
    }
}
