using System.Diagnostics;

namespace Duck.Platform.Default;

internal class FrameTimer : IFrameTimer
{

    #region Properties

    public float Delta { get; private set; }
    public double DoubleDelta { get; private set; }

    #endregion

    #region Members

    private Stopwatch _timer;
    private double _previousTime;

    #endregion

    #region Methods

    public FrameTimer()
    {
        _timer = new Stopwatch();

        Reset();
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Update()
    {
        var time = _timer.Elapsed.TotalSeconds;

        DoubleDelta = time - _previousTime;
        Delta = (float)DoubleDelta;

        _previousTime = time;
    }

    private void Reset()
    {
        _previousTime = _timer.Elapsed.TotalSeconds;
    }

    #endregion
}
