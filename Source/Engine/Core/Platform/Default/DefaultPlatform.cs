using System.Diagnostics;

namespace Duck.Platform.Default;

public class DefaultPlatform : IPlatform
{
    #region IPlatform implementation

    public IWindow? Window => _window;

    #endregion

    #region Members

    private IWindow? _window;
    private IFrameTimer? _frameTimer;

    #endregion

    #region Methods

    public DefaultPlatform()
    {
    }

    public void Initialize()
    {
        _window = new DefaultWindow(
            Configuration.Default
        );

        _frameTimer = new FrameTimer();

        Time.FrameTimer = _frameTimer;
    }

    public void Tick()
    {
        _window?.Update();
    }

    public void PostTick()
    {
        _window?.ClearEvents();
    }

    public void Render()
    {
        _window?.Render();
    }

    #endregion
}
