using Duck.Serialization;

namespace Duck;

public interface IApplication
{
    public bool Initialize();

    public void Run();

    public void Shutdown();

    public T GetModule<T>() where T : IModule;
}

public interface IModule
{
}

public interface IInitializableModule : IModule
{
    public bool Init();
}

public interface ITickableModule : IModule
{
    public void Tick();
}

public interface IPreTickableModule : IModule
{
    public void PreTick();
}

public interface IPostTickableModule : IModule
{
    public void PostTick();
}

public interface IPreRenderableModule : IModule
{
    public void PreRender();
}

public interface IRenderableModule : IModule
{
    public void Render();
}

public interface IPostRenderableModule : IModule
{
    public void PostRender();
}

public interface IShutdownModule : IModule
{
    public void Shutdown();
}

public interface IHotReloadAwareModule : IModule
{
    public void BeginHotReload();
    public void EndHotReload();
}

public interface IHotReloadContext
{
    public ISerializer Serializer { get; }
}
