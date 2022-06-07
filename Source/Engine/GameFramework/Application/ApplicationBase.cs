using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Exceptions;
using Duck.Graphics;
using Duck.Graphics.OpenGL;
using Duck.Input;
using Duck.Logging;
using Duck.Physics;
using Duck.Physics.Systems;
using Duck.Platform;
using Duck.Scene;
using Duck.Scene.Systems;
using Duck.Serialization;
using Duck.ServiceBus;

namespace Duck.GameFramework;

public abstract class ApplicationBase : IApplication
{
    #region Members

    private readonly IPlatform _platform;
    private readonly List<IModule> _modules = new();

    private State _state = State.Uninitialized;
    private ILogger _systemLogger;

    private bool _isEditor;
    private bool _isHotReloading;

    #endregion

    public ApplicationBase(bool isEditor)
    {
        _platform = new OpenGLPlatform(this);
        _isEditor = isEditor;

        RegisterModules();
    }

    public bool Initialize()
    {
        if (_state != State.Uninitialized) {
            throw new Exception("Application already initialized");
        }

        ChangeState(State.Initializing);

        Instanciator.Init();

        Time.FrameTimer = _platform.CreateFrameTimer();

        if (!InitializeModules()) {
            return false;
        }

        ChangeState(State.Initialized);

        return true;
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

        IterateOverModules<IHotReloadAwareModule>(module => {
            if (module is not ISerializable serializable) {
                _systemLogger.LogError("Hot reloadable module is not serializable: " + module.GetType().Name);
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
        IterateOverModules<IHotReloadAwareModule>(module => module.EndHotReload());
    }

    private bool InitializeModules()
    {
        foreach (var module in _modules) {
            if (module is IInitializableModule initModule) {
                if (!initModule.Init()) {
                    return false;
                }
            }
        }

        _systemLogger = GetModule<ILogModule>().CreateLogger("System");

        return true;
    }

    private void PreTickModules()
    {
        IterateOverModules<IPreTickableModule>(module => module.PreTick());
    }

    private void TickModules()
    {
        GetModule<IEventBus>().Emit();

        IterateOverModules<ITickableModule>(module => module.Tick());
    }

    private void PostTickModules()
    {
        IterateOverModules<IPostTickableModule>(module => module.PostTick());
    }

    private void PreRenderModules()
    {
        IterateOverModules<IPreRenderableModule>(module => module.PreRender());
    }

    private void RenderModules()
    {
        IterateOverModules<IRenderableModule>(module => module.Render());
    }

    private void PostRenderModules()
    {
        IterateOverModules<IPostRenderableModule>(module => module.PostRender());
    }

    private void IterateOverModules<T>(Action<T> callback)
    {
        foreach (var module in _modules) {
            if (module is T cast) {
                callback(cast);
            }
        }
    }

    protected virtual void RegisterModules()
    {
        AddModule(new LogModule());
        AddModule(new EventBus());
        AddModule(new ContentModule(GetModule<ILogModule>()));
        AddModule(new GraphicsModule(this, _platform, GetModule<ILogModule>(), GetModule<IContentModule>()));
        AddModule(new EcsModule(GetModule<ILogModule>(), GetModule<IEventBus>()));
        AddModule(new SceneModule(GetModule<IEcsModule>(), GetModule<IGraphicsModule>(), GetModule<IContentModule>()));
        AddModule(new InputModule(GetModule<ILogModule>(), _platform));
        AddModule(new PhysicsModule(GetModule<ILogModule>(), GetModule<IEventBus>()));
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

            _platform.Tick();

            PreTickModules();
            TickModules();
            PostTickModules();

            _platform.PostTick();

            PreRenderModules();
            RenderModules();
            PostRenderModules();

            Thread.Sleep(16);
        }
    }

    public void Shutdown()
    {
        if (_state == State.TearingDown) {
            return;
        }

        _systemLogger?.LogInformation("Shutdown requested");

        ChangeState(State.TearingDown);
    }

    public void AddModule(IModule module)
    {
        _modules.Add(module);
    }

    public ISystemComposition CreateDefaultSystemComposition(IScene scene)
    {
        var c = new SystemComposition(scene.World);
        c
            .Add(new CameraSystem(scene, GetModule<IGraphicsModule>()))
            .Add(new MeshLoadSystem(scene, GetModule<IContentModule>(), GetModule<IGraphicsModule>()))
            .Add(new MeshRenderSystem(scene, GetModule<IGraphicsModule>()))
            .Add(new RigidBodyLifecycleSystem_AddBox(scene.World, GetModule<IPhysicsModule>()))
            .Add(new RigidBodyLifecycleSystem_RemoveBox(scene.World, GetModule<IPhysicsModule>()))
            .Add(new RigidBodyLifecycleSystem_AddSphere(scene.World, GetModule<IPhysicsModule>()))
            .Add(new RigidBodyLifecycleSystem_RemoveSphere(scene.World, GetModule<IPhysicsModule>()))
            .Add(new RigidBodySynchronizationSystem(scene.World, GetModule<IPhysicsModule>()));

        return c;
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
