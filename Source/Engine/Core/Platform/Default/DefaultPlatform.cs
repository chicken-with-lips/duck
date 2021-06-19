using System.Diagnostics;

namespace Duck.Platform.Default;

public class DefaultPlatform : IPlatform
{
    #region IPlatform implementation

    public IWindow Window {
        get {
            Debug.Assert(_window != null, "Platform needs to be initialized before use.");

            return _window;
        }
    }

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
        _frameTimer = new FrameTimer(_window);

        Time.FrameTimer = _frameTimer;
    }

    #endregion
}
