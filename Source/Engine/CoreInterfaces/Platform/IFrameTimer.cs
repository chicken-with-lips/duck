namespace Duck.Platform;

public interface IFrameTimer
{
    public float Delta { get; }
    public double DoubleDelta { get; }

    public void Start();
    public void Update();
}
