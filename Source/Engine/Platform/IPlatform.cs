namespace Duck.Platform;

public interface IPlatform
{
    FrameTimer CreateFrameTimer();
    IWindow CreateWindow(in WindowConfiguration? config = null);

    IWindow PrimaryWindow { get; }
    IView PrimaryView { get; }

    bool Initialize(IApplication app);
    void Shutdown();
    bool Update();
}