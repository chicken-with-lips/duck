namespace Duck.Platform.Default;

internal class FrameTimer : IFrameTimer
{

    #region Properties

    public float Delta { get; private set; }
    public double DoubleDelta { get; private set; }

    #endregion

    #region Members

    private double _previousTime;
    private readonly IWindow _window;

    #endregion

    #region Methods

    public FrameTimer(IWindow window)
    {
        _window = window;

        Reset();
    }

    public void Reset()
    {
        _previousTime = _window.ElapsedTime;
    }

    public void Update()
    {
        var time = _window.ElapsedTime;

        DoubleDelta = time - _previousTime;
        Delta = (float)DoubleDelta;

        _previousTime = time;
    }

    #endregion
}
