namespace Duck.Platform;

public class FrameAccumulator
{
    public float TargetFrameRate { get; set; } = 60f;
    public float TargetFrameRateStepDuration => 1f / TargetFrameRate;

    private readonly FrameTimer _timer;
    private float _accumulator;

    public FrameAccumulator(FrameTimer timer)
    {
        _timer = timer;
    }

    public void Update()
    {
        // https://gafferongames.com/post/fix_your_timestep/
        _accumulator += _timer.Delta;

        if (_accumulator > 10) {
            // application probably lost control, fixed update would take too long
            Reset();
        }
    }

    public void Reset()
    {
        _accumulator = 0;
    }

    public bool Consume()
    {
        if (_accumulator >= TargetFrameRateStepDuration) {
            _accumulator -= TargetFrameRateStepDuration;

            return true;
        }

        return false;
    }
}