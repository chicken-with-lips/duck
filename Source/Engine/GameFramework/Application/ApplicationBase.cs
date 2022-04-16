using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Exceptions;
using Duck.Input;
using Duck.Logging;
using Duck.Physics;
using Duck.Platform;
using Duck.Platform.Default;
using Duck.Scene;
using Duck.Serialization;
using Duck.ServiceBus;

namespace Duck.GameFramework;

public abstract class ApplicationBase : IApplication
{
    #region Members

    private State _state = State.Uninitialized;
    private ILogger _systemLogger;
    private readonly IPlatform _platform;

    private readonly List<IApplicationSubsystem> _subsystems = new();

    private bool _isEditor;
    private bool _isHotReloading;

    #endregion

    public ApplicationBase(bool isEditor)
    {
        _platform = new DefaultPlatform();
        _isEditor = isEditor;

        RegisterSubsystems();
    }

    public bool Initialize()
    {
        if (_state != State.Uninitialized) {
            throw new Exception("Application already initialized");
        }

        ChangeState(State.Initializing);

        Instanciator.Init();

        _platform.Initialize();

        if (!InitializeSubsystems()) {
            return false;
        }

        ChangeState(State.Initialized);

        return true;
    }

    public T GetSubsystem<T>() where T : IApplicationSubsystem
    {
        foreach (var subsystem in _subsystems) {
            if (subsystem is T applicationSubsystem) {
                return applicationSubsystem;
            }
        }

        throw new ApplicationSubsystemNotFoundException();
    }

    public IHotReloadContext BeginHotReload()
    {
        var serializationContext = new SerializationContext(true);
        var serializer = new GraphSerializer(serializationContext);

        IterateOverSystems<IHotReloadAwareSubsystem>(subsystem => {
            if (subsystem is not ISerializable serializable) {
                _systemLogger.LogError("Hot reloadable system is not serializable: " + subsystem.GetType().Name);
            } else {
                serializable.Serialize(serializer, serializationContext);
                subsystem.BeginHotReload();
            }
        });

        serializer.Close();

        return new HotReloadContext(serializer);
    }

    public void EndHotReload(IHotReloadContext context)
    {
        IterateOverSystems<IHotReloadAwareSubsystem>(subsystem => subsystem.EndHotReload());
    }

    private bool InitializeSubsystems()
    {
        foreach (var subsystem in _subsystems) {
            if (subsystem is IApplicationInitializableSubsystem applicationSubsystem) {
                if (!applicationSubsystem.Init()) {
                    return false;
                }
            }
        }

        _systemLogger = GetSubsystem<ILogSubsystem>().CreateLogger("System");

        return true;
    }

    private void PreTickSubsystems()
    {
        IterateOverSystems<IApplicationPreTickableSubsystem>(subsystem => subsystem.PreTick());
    }

    private void TickSubsystems()
    {
        GetSubsystem<IEventBus>().Emit();

        IterateOverSystems<IApplicationTickableSubsystem>(subsystem => subsystem.Tick());
    }

    private void PostTickSubsystems()
    {
        IterateOverSystems<IApplicationPostTickableSubsystem>(subsystem => subsystem.PostTick());
    }

    private void RenderSubsystems()
    {
        IterateOverSystems<IApplicationRenderableSubsystem>(subsystem => subsystem.Render());
    }

    private void IterateOverSystems<T>(Action<T> callback)
    {
        foreach (var subsystem in _subsystems) {
            if (subsystem is T applicationSubsystem) {
                callback(applicationSubsystem);
            }
        }
    }

    protected virtual void RegisterSubsystems()
    {
        AddSubsystem(new LogSubsystem());
        AddSubsystem(new EventBus());
        AddSubsystem(new GraphicsSubsystem(this, GetSubsystem<ILogSubsystem>()));
        AddSubsystem(new ContentSubsystem(GetSubsystem<ILogSubsystem>()));
        AddSubsystem(new WorldSubsystem(GetSubsystem<ILogSubsystem>(), GetSubsystem<IEventBus>()));
        AddSubsystem(new SceneSubsystem(GetSubsystem<IWorldSubsystem>()));
        AddSubsystem(new InputSubsystem(GetSubsystem<ILogSubsystem>(), _platform));
        AddSubsystem(new PhysicsSubsystem(GetSubsystem<ILogSubsystem>(), GetSubsystem<IEventBus>()));
    }

    public void Run()
    {
        if (_state != State.Initialized) {
            throw new Exception("Application has not been initialized");
        }

        ChangeState(State.Running);

        Time.FrameTimer?.Start();

        while (_state is State.Running or State.HotReloading) {
            // if (_platform.Window.CloseRequested) {
            // Shutdown();
            // break;
            // }

            Time.FrameTimer?.Update();

            _platform.Tick();

            PreTickSubsystems();
            TickSubsystems();
            PostTickSubsystems();

            _platform.PostTick();

            RenderSubsystems();

            _platform.Render();

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

    public void AddSubsystem(IApplicationSubsystem subsystem)
    {
        _subsystems.Add(subsystem);
    }

    public ISystemComposition CreateDefaultSystemComposition(IScene scene)
    {
        var c = new SystemComposition(scene.World);
        // c
        // .Add(new GenerateFilamentIdentitySystem())
        // .Add(new CameraLifecycleSystem())
        // .Add(new BoxPrimitiveLifecycleSystem())
        // .Add(new MeshLifecycleSystem())
        // .Add(new PhysicsBoxShapeLifecycleSystem())
        // .Add(new SyncPhysicsTransformsSystem())
        // .Add(new SyncRenderTransformsSystem());

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
