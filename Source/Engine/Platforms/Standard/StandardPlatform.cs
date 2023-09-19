using Duck.Platform;

namespace Duck.Platforms.Standard;

public class StandardPlatform : IPlatform, IPreTickableModule, ITickableModule, IPostTickableModule
{
    #region Properties

    public IWindow[] Windows => _windows.ToArray();

    #endregion

    #region Members

    private readonly List<IWindow> _windows = new();

    private bool _hasInitializedContent = false;

    #endregion

    #region Methods

    public IWindow CreateWindow()
    {
        var window = new StandardWindow(
            Configuration.Default
        );

        _windows.Add(window);

        return window;
    }

    public IFrameTimer CreateFrameTimer()
    {
        return new FrameTimer();
    }

    public void PreTick()
    {
        foreach (var window in _windows) {
            window.ClearEvents();
            window.Update();
        }
    }

    public void Tick()
    {
    }

    public void PostTick()
    {
    }

    #endregion
}
