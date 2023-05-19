using Arch.Core;
using Duck.Content;
using Duck.Exceptions;
using Duck.Input;
using Duck.Logging;
using Duck.Physics;
using Duck.Physics.Systems;
using Duck.Platform;
using Duck.Renderer;
using Duck.Renderer.Systems;
using Duck.Scene.Systems;
using Duck.Serialization;
using Duck.ServiceBus;
using Duck.Ui;
using Duck.Ui.Systems;
using Tracy.Net;

namespace Duck.GameFramework;

public abstract class ApplicationBase : IApplication
{
    #region Members

    private readonly IPlatform _platform;
    private readonly IRenderSystem _renderSystem;
    private readonly List<IModule> _modules = new();

    private State _state = State.Uninitialized;
    private ILogger? _systemLogger;

    private bool _isEditor;
    private bool _isHotReloading;

    private float _deltaTimeAccumulator;
    private bool _shouldSkipFrames = true;

    #endregion

    public ApplicationBase(IPlatform platform, IRenderSystem renderSystem, bool isEditor)
    {
        _platform = platform;
        _renderSystem = renderSystem;
        _isEditor = isEditor;
    }

    public bool Initialize()
    {
        if (_state != State.Uninitialized) {
            throw new Exception("Application already initialized");
        }

        TracyClient.ZoneBegin("Application::Init");

        ChangeState(State.Initializing);

        Instanciator.Init();
        RegisterModules();

        Time.FrameTimer = _platform.CreateFrameTimer();

        InitializeApp();

        if (!InitializeModules()) {
            return false;
        }

        ChangeState(State.Initialized);

        TracyClient.ZoneEnd();

        return true;
    }

    protected virtual void InitializeApp()
    {
    }

    public T GetModule<T>() where T : IModule
    {
        foreach (var module in _modules) {
            if (module is T cast) {
                return cast;
            }
        }

        throw new ApplicationModuleNotFoundException();
    }

    public IHotReloadContext BeginHotReload()
    {
        var serializationContext = new SerializationContext(true);
        var serializer = new GraphSerializer(serializationContext);

        IterateOverModules<IHotReloadAwareModule>("HotReload", module => {
            if (module is not ISerializable serializable) {
                _systemLogger?.LogError("Hot reloadable module is not serializable: " + module.GetType().Name);
            } else {
                serializable.Serialize(serializer, serializationContext);
                module.BeginHotReload();
            }
        });

        serializer.Close();

        return new HotReloadContext(serializer);
    }

    public void EndHotReload(IHotReloadContext context)
    {
        IterateOverModules<IHotReloadAwareModule>("EndHotReload", module => module.EndHotReload());
    }

    private bool InitializeModules()
    {
        foreach (var module in _modules) {
            if (module is IInitializableModule initModule) {
                TracyClient.ZoneBegin("Init::" + module.GetType().FullName);

                var result = initModule.Init();

                TracyClient.ZoneEnd();

                if (!result) {
                    return false;
                }
            }
        }

        _systemLogger = GetModule<ILogModule>().CreateLogger("System");

        return true;
    }

    private void PreTickModules()
    {
        _platform.PreTick();

        IterateOverModules<IPreTickableModule>("PreTick", module => module.PreTick());
    }

    private void TickModules()
    {
        _platform.Tick();

        GetModule<IEventBus>().Flush();

        IterateOverModules<ITickableModule>("Tick", module => module.Tick());
    }

    private void FixedTickModules()
    {
        GetModule<IEventBus>().Flush();

        // borrowed from wicked engine
        if (_shouldSkipFrames) {
            _deltaTimeAccumulator += Time.DeltaFrame;

            if (_deltaTimeAccumulator > 10) {
                // application probably lost control, fixed update would take too long
                _deltaTimeAccumulator = 0;
            }

            float targetFrameRateInv = 1.0f / (Time.FrameTimer?.TargetFrameRate ?? 0);

            while (_deltaTimeAccumulator >= targetFrameRateInv) {
                IterateOverModules<IFixedTickableModule>("FixedTick", module => module.FixedTick());
                _deltaTimeAccumulator -= targetFrameRateInv;
            }
        } else {
            IterateOverModules<IFixedTickableModule>("FixedTick", module => module.FixedTick());
        }
    }

    private void PostTickModules()
    {
        _platform.PostTick();

        IterateOverModules<IPostTickableModule>("PostTick", module => module.PostTick());
    }

    private void PreRenderModules()
    {
        IterateOverModules<IPreRenderableModule>("PreRender", module => module.PreRender());
    }

    private void RenderModules()
    {
        IterateOverModules<IRenderableModule>("Render", module => module.Render());
    }

    private void PostRenderModules()
    {
        IterateOverModules<IPostRenderableModule>("PostRender", module => module.PostRender());
    }

    private void ShutdownModules()
    {
        IterateOverModules<IShutdownModule>("Shutdown", module => module.Shutdown());
    }

    private void IterateOverModules<T>(string zoneName, Action<T> callback)
    {
        foreach (var module in _modules) {
            if (module is T cast) {
                TracyClient.Zone<T>(zoneName + "::" + module.GetType().Name, callback, cast);
            }
        }
    }

    protected virtual void RegisterModules()
    {
        AddModule(new LogModule());
        AddModule(new EventBus());
        AddModule(new ContentModule(GetModule<ILogModule>()));
        AddModule(new RendererModule(this, _platform, GetModule<IEventBus>(), _renderSystem, GetModule<ILogModule>(), GetModule<IContentModule>()));
        AddModule(new InputModule(GetModule<ILogModule>(), _platform));
        AddModule(new PhysicsModule(GetModule<ILogModule>(), GetModule<IEventBus>()));
        AddModule(new UiModule(GetModule<ILogModule>(), GetModule<IRendererModule>(), GetModule<IContentModule>(), GetModule<IInputModule>()));
    }

    public void Run()
    {
        if (_state != State.Initialized) {
            throw new Exception("Application has not been initialized");
        }

        ChangeState(State.Running);

        Time.FrameTimer?.Start();

        while (_state is State.Running or State.HotReloading) {
            Time.FrameTimer?.Update();

            PreTickModules();
            FixedTickModules();
            TickModules();
            PostTickModules();

            PreRenderModules();
            RenderModules();
            PostRenderModules();

            TracyClient.FrameMark();
        }
    }

    public void Shutdown()
    {
        if (_state == State.TearingDown) {
            return;
        }

        _systemLogger?.LogInformation("Shutdown requested");

        ChangeState(State.TearingDown);
        ShutdownModules();
    }

    public void AddModule(IModule module)
    {
        _modules.Add(module);
    }

    public void PopulateSystemCompositionWithDefaults(World world, SystemRoot composition)
    {
        composition.SimulationGroup
            .Add(new RigidBodySynchronizationSystem(world))
            .Add(new RigidBodyLifecycleSystem(world, GetModule<IPhysicsModule>()))
            .Add(new CameraSystem(world, GetModule<IRendererModule>()))
            .Add(new LoadStaticMeshSystem(world, GetModule<IContentModule>()))
            .Add(new ContextLoadSystem(world, GetModule<UiModule>()))
            .Add(new UserInterfaceLoadSystem(world, GetModule<IContentModule>(), GetModule<UiModule>()))
            .Add(new UserInterfaceTickSystem(world));

        composition.LateSimulationGroup
            .Add(new ContextSyncSystem(world, GetModule<UiModule>()));

        composition.PresentationGroup
            .Add(new RenderSceneSystem(world, GetModule<IRendererModule>().GraphicsDevice))
            .Add(new UserInterfaceRenderSystem(world, GetModule<UiModule>()));
    }

    private void ChangeState(State newState)
    {
        _state = newState;
    }

    public enum State
    {
        Uninitialized,
        Initializing,
        Initialized,
        Running,
        HotReloading,
        TearingDown,
    }
}

public class HotReloadContext : IHotReloadContext
{
    public ISerializer Serializer { get; }

    public HotReloadContext(ISerializer serializer)
    {
        Serializer = serializer;
    }
}
