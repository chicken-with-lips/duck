using Duck.Logging;
using Duck.Platform;

namespace Duck.Graphics;

public class GraphicsSubsystem : IGraphicsSubsystem, IApplicationTickableSubsystem
{
    #region Members

    private readonly ILogger _logger;
    private readonly IApplication _app;
    private readonly IPlatform _platform;

    #endregion

    #region Methods

    internal GraphicsSubsystem(IApplication app, ILogSubsystem logSubsystem, IPlatform platform)
    {
        _app = app;
        _platform = platform;

        _logger = logSubsystem.CreateLogger("Graphics");
        _logger.LogInformation("Initializing graphics subsystem.");
    }

    #endregion

    #region IRenderingSubsystem

    public void Tick()
    {
        ProcessWindowEvents();
    }

    private void ProcessWindowEvents()
    {
        // foreach (var windowEvent in _nativeWindow.Events) {
        // if (windowEvent is NativeWindow.ResizeEvent resizeEvent) {
        // _renderingWindow?.Resize(resizeEvent.NewWidth, resizeEvent.NewHeight);
        // }
        // }
    }

    #endregion
}
