namespace Duck.Platform;

public interface IFrameTimer
{
    public float Elapsed { get; }
    public float Delta { get; }
    public double DoubleDelta { get; }
    
    public bool IsFrameLockEnabled { get; set; }
    public float TargetFrameRate { get; set; }

    public void Start();
    public void Update();
}
