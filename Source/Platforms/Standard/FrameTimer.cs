using System.Diagnostics;
using Duck.Platform;

namespace Duck.Platforms.Standard;

public class FrameTimer : IFrameTimer
{
    #region Properties

    public float Elapsed { get; private set; }
    public float Delta { get; private set; }
    public double DoubleDelta { get; private set; }

    public bool IsFrameLockEnabled { get; set; } = false;
    public float TargetFrameRate { get; set; } = 60;

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

        var deltaTime = time - _previousTime;
        deltaTime = IsFrameLockEnabled ? (1.0f / TargetFrameRate) : deltaTime;

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
