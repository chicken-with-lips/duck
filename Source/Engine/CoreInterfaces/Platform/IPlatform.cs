namespace Duck.Platform;

public interface IPlatform
{
    public IWindow? Window { get; }

    public void Initialize();
    public void Tick();
    public void PostTick();
    public void Render();
}
