using System.Diagnostics;

namespace Duck.Platform;

public class FrameTimer
{
    #region Properties

    public float Elapsed { get; private set; }
    public float Delta { get; private set; }
    public double DoubleDelta { get; private set; }

    #endregion

    #region Members

    private readonly Stopwatch _timer;
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
        var deltaTime = time - _previousTime;

        Elapsed = (float)time;
        DoubleDelta = deltaTime;
        Delta = (float)DoubleDelta;

        _previousTime = time;
    }

    private void Reset()
    {
        _previousTime = _timer.Elapsed.TotalSeconds;
    }

    #endregion
}
