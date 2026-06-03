using Duck.Scene;

namespace Duck.Platform;

public interface IPlatform
{
    public FrameTimer CreateFrameTimer();
    public IWindow CreateWindow(in WindowConfiguration? config = null);
    
    public IWindow PrimaryWindow { get; }
    public IView PrimaryView { get; }

    public void Initialize(IApplication app);
    public void Shutdown();
    public bool Update();
}
