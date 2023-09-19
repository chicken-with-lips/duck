using Duck.Serialization;

namespace Duck;

public interface IApplication
{
    public bool IsInPlayMode { get; }

    public IModule[] Modules { get; }

    public bool Initialize();

    public void RunFrame();

    public void Shutdown();

    public T GetModule<T>() where T : IModule;

    public void ChangeState(ApplicationState newState);

    public void EnterPlayMode();
    public void ExitPlayMode();
}


public enum ApplicationState
{
    Uninitialized,
    Initializing,
    Initialized,
    Running,
    HotReloading,
    TearingDown,
}

public interface IModule
{
}

public interface IInitializableModule : IModule
{
    public bool Init();
}

public interface IPostInitializableModule : IModule
{
    public void PostInit();
}

public interface ITickableModule : IModule
{
    public void Tick();
}

public interface IFixedTickableModule : IModule
{
    public void FixedTick();
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

public interface IEnterPlayModeModule : IModule
{
    public void EnterPlayMode();
}

public interface IExitPlayModeModule : IModule
{
    public void ExitPlayMode();
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

public interface IModuleCanBeInstanced
{
    public IModule CreateModuleInstance(IApplication app);
}
