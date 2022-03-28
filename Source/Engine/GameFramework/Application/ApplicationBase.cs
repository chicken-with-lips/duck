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

    private bool InitializeSubsystems()
    {
        foreach (var subsystem in _subsystems) {
            if (subsystem is IApplicationInitializableSubsystem initializable) {
                if (!initializable.Init()) {
                    return false;
                }
            }
        }

        _systemLogger = GetSubsystem<ILogSubsystem>().CreateLogger("System");

        return true;
    }

    private void PreTickSubsystems()
    {
        foreach (var subsystem in _subsystems) {
            if (subsystem is IApplicationPreTickableSubsystem tickable) {
                tickable.PreTick();
            }
        }
    }

    private void TickSubsystems()
    {
        GetSubsystem<IEventBus>().Emit();

        foreach (var subsystem in _subsystems) {
            if (subsystem is IApplicationTickableSubsystem tickable) {
                tickable.Tick();
            }
        }
    }

    private void PostTickSubsystems()
    {
        foreach (var subsystem in _subsystems) {
            if (subsystem is IApplicationPostTickableSubsystem tickable) {
                tickable.PostTick();
            }
        }
    }

    protected virtual void RegisterSubsystems()
    {
        AddSubsystem(new LogSubsystem());
        AddSubsystem(new EventBus());
        // AddSubsystem(new RenderingSubsystem(this, GetSubsystem<ILogSubsystem>()));
        AddSubsystem(new ContentSubsystem(GetSubsystem<ILogSubsystem>()));
        AddSubsystem(new WorldSubsystem(GetSubsystem<ILogSubsystem>(), GetSubsystem<IEventBus>()));
        AddSubsystem(new SceneSubsystem(GetSubsystem<IWorldSubsystem>()));
        AddSubsystem(new InputSubsystem(GetSubsystem<ILogSubsystem>(), _platform));
        AddSubsystem(new PhysicsSubsystem(GetSubsystem<ILogSubsystem>(), GetSubsystem<IEventBus>()));

        if (_isEditor) {
        } else {
        }
    }

    public void Run()
    {
        if (_state != State.Initialized) {
            throw new Exception("Application has not been initialized");
        }

        ChangeState(State.Running);

        Time.FrameTimer?.Start();

        while (_state == State.Running) {
            // if (_platform.Window.CloseRequested) {
            // Shutdown();
            // break;
            // }

            Time.FrameTimer?.Update();

            PreTickSubsystems();
            TickSubsystems();

            _platform.Window?.ClearEvents();

            PostTickSubsystems();
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
        TearingDown,
    }
}
