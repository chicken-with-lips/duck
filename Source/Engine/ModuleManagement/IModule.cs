using Duck.Platform;

namespace Duck.ModuleManagement;

public interface IModule
{
}

public interface IInitializableModule : IModule
{
    void Initialize(IApplication app, IInitializationContext context);
}

public interface IShutdownModule : IModule
{
    void Shutdown(IApplication app);
}

public interface ITickModule : IModule
{
    void Tick(FrameTimer frameTimer);
}

public interface IFixedTickModule : IModule
{
    void FixedTick(FrameTimer frameTimer);
}

public interface IPreTickModule : IModule
{
    void PreTick(FrameTimer frameTimer);
}

public interface IPostTickModule : IModule
{
    void PostTick(FrameTimer frameTimer);
}

public interface IPresentSingleThreadedModule : IModule
{
    void Present(FrameTimer frameTimer);
}

public interface ISingleThreadedModule : IModule
{
}

public interface IHotReloadSubscriberModule : IModule
{
    void BeginHotReload(HotReloadInstigator[] instigators);
    void EndHotReload();
}

public interface IInitializationContext
{
    bool WasHotReloaded { get; }
}

public class InitializationContext : IInitializationContext
{
    public bool WasHotReloaded { get; }

    public InitializationContext(bool wasHotReloaded)
    {
        WasHotReloaded = wasHotReloaded;
    }
}
