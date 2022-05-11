using System.Diagnostics;
using Duck.Platform;

namespace Duck.Graphics.OpenGL;

internal class OpenGLFrameTimer : IFrameTimer
{
    #region Properties

    public float Elapsed { get; private set; }
    public float Delta { get; private set; }
    public double DoubleDelta { get; private set; }

    #endregion

    #region Members

    private Stopwatch _timer;
    private double _previousTime;

    #endregion

    #region Methods

    public OpenGLFrameTimer()
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

        Elapsed = (float)time;
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
