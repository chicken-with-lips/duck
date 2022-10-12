using Duck.Content;
using Duck.Ecs;
using Duck.Exceptions;
using Duck.Graphics;
using Duck.Graphics.OpenGL;
using Duck.Graphics.Systems;
using Duck.Input;
using Duck.Logging;
using Duck.Physics;
using Duck.Physics.Systems;
using Duck.Platform;
using Duck.Scene;
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
    private readonly List<IModule> _modules = new();

    private State _state = State.Uninitialized;
    private ILogger? _systemLogger;

    private bool _isEditor;
    private bool _isHotReloading;

    #endregion

    public ApplicationBase(bool isEditor)
    {
        _platform = new OpenGLPlatform(this);
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

        GetModule<IEventBus>().Emit();

        IterateOverModules<ITickableModule>("Tick", module => module.Tick());
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
        AddModule(new GraphicsModule(this, _platform, GetModule<ILogModule>(), GetModule<IContentModule>()));
        AddModule(new InputModule(GetModule<ILogModule>(), _platform));
        AddModule(new EcsModule(GetModule<ILogModule>(), GetModule<IEventBus>()));
        AddModule(new PhysicsModule(GetModule<ILogModule>(), GetModule<IEventBus>()));
        AddModule(new UiModule(GetModule<ILogModule>(), GetModule<IGraphicsModule>(), GetModule<IContentModule>(), GetModule<IInputModule>()));
        AddModule(new SceneModule(GetModule<IEcsModule>(), GetModule<IGraphicsModule>(), GetModule<IEventBus>()));
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
            TickModules();
            PostTickModules();

            PreRenderModules();
            RenderModules();
            PostRenderModules();

            Thread.Sleep(16);

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

    public void PopulateSystemCompositionWithDefaults(IScene scene, ISystemComposition composition)
    {
        composition
            .Add(new CameraSystem(scene, GetModule<IGraphicsModule>()))
            .Add(new MeshLoadSystem(scene, GetModule<IContentModule>(), GetModule<IGraphicsModule>()))
            .Add(new ContextLoadSystem(scene.World, GetModule<UiModule>()))
            .Add(new UserInterfaceLoadSystem(scene.World, GetModule<IContentModule>(), GetModule<UiModule>()))
            .Add(new RigidBodyLifecycleSystem_AddBox(scene.World, GetModule<IPhysicsModule>()))
            .Add(new RigidBodyLifecycleSystem_RemoveBox(scene.World, GetModule<IPhysicsModule>()))
            .Add(new RigidBodyLifecycleSystem_AddSphere(scene.World, GetModule<IPhysicsModule>()))
            .Add(new RigidBodyLifecycleSystem_RemoveSphere(scene.World, GetModule<IPhysicsModule>()))
            .Add(new RigidBodySynchronizationSystem(scene.World, GetModule<IPhysicsModule>()))
            .Add(new UserInterfaceTickSystem(scene.World))
            .Add(new ContextSyncSystem(scene.World, GetModule<UiModule>()))
            .Add(new ScheduleRenderableSystem(scene.World, GetModule<IGraphicsModule>()))
            .Add(new UserInterfaceRenderSystem(scene.World, GetModule<IContentModule>(), GetModule<UiModule>()));
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
