namespace Duck.Platform;

public interface IPlatform
{
    public IWindow? Window { get; }

    public void Initialize();
}
