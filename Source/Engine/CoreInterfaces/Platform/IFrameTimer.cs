namespace Duck.Platform;

public interface IFrameTimer
{
    public float Delta { get; }
    public double DoubleDelta { get; }

    public void Reset();
    public void Update();
}
