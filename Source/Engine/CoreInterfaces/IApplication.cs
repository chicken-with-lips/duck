using Duck.Serialization;

namespace Duck;

public interface IApplication
{
    public bool Initialize();

    public void Run();

    public void Shutdown();

    public T GetSubsystem<T>() where T : IApplicationSubsystem;
}

public interface IApplicationSubsystem
{
}

public interface IApplicationInitializableSubsystem : IApplicationSubsystem
{
    public bool Init();
}

public interface IApplicationTickableSubsystem : IApplicationSubsystem
{
    public void Tick();
}

public interface IApplicationPreTickableSubsystem : IApplicationSubsystem
{
    public void PreTick();
}

public interface IApplicationPostTickableSubsystem : IApplicationSubsystem
{
    public void PostTick();
}

public interface IApplicationRenderableSubsystem : IApplicationSubsystem
{
    public void Render();
}

public interface IHotReloadAwareSubsystem : IApplicationSubsystem
{
    public void BeginHotReload();
    public void EndHotReload();
}

public interface IHotReloadContext
{
    public ISerializer Serializer { get; }
}
