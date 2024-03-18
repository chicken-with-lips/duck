namespace Duck.Platform;

public interface IPlatform : IModule
{
    public IWindow[] Windows { get; }
    
    public void PreTick();
    public void Tick();
    public void PostTick();

    public IFrameTimer CreateFrameTimer();
    public IWindow CreateWindow();
}
