using Duck.Audio;
using Duck.Content;
using Duck.GameFramework.Exceptions;
using Duck.Input;
using Duck.Logging;
using Duck.Physics;
using Duck.Platform;
using Duck.Graphics;
using Duck.Serialization;
using Duck.ServiceBus;
using Duck.Ui;
using Tracy;

namespace Duck.GameFramework;

public abstract class ApplicationBase : IApplication
{
    #region Properties

    public IModule[] Modules => _modules.ToArray();
    public bool IsInPlayMode => _isInPlayMode;

    #endregion

    #region Members

    private readonly IPlatform _platform;
    private readonly IRenderSystem _renderSystem;
    private readonly List<IModule> _modules = new();

    private ApplicationState _state = ApplicationState.Uninitialized;
    private ILogger? _systemLogger;

    private bool _isEditor;
    private bool _isHotReloading;
    private bool _isInPlayMode;

    private float _deltaTimeAccumulator;
    private bool _shouldSkipFrames = true;

    #endregion

    public ApplicationBase(IPlatform platform, IRenderSystem renderSystem, bool isEditor)
    {
        _platform = platform;
        _renderSystem = renderSystem;
        _isEditor = isEditor;
    }

    public virtual bool Initialize()
    {
        if (_state != ApplicationState.Uninitialized) {
            throw new Exception("Application already initialized");
        }

        TracyClient.ZoneBegin("Application::Init");

        ChangeState(ApplicationState.Initializing);

        Instanciator.Init();
        RegisterModules();

        Time.FrameTimer = _platform.CreateFrameTimer();

        InitializeApp();

        if (!InitializeModules()) {
            return false;
        }

        ChangeState(ApplicationState.Initialized);

        TracyClient.ZoneEnd();

        return true;
    }

    protected virtual void InitializeApp()
    {
    }

    public virtual T GetModule<T>() where T : IModule
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
            // if (module is not ISerializable serializable) {
            // _systemLogger?.LogError("Hot reloadable module is not serializable: " + module.GetType().Name);
            // } else {
            // serializable.Serialize(serializer, serializationContext);
            module.BeginHotReload();
            // }
        });

        serializer.Close();

        return new HotReloadContext(serializer);
    }

    public void EndHotReload(IHotReloadContext context)
    {
        IterateOverModules<IHotReloadAwareModule>("EndHotReload", module => module.EndHotReload());
    }

    public IApplication CreateProxy(bool isEditor)
    {
        return new InstancedApplication(this, _platform, _renderSystem, isEditor);
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

        IterateOverModules<IPostInitializableModule>("PostInit", module => module.PostInit());

        _systemLogger = GetModule<ILogModule>().CreateLogger("System");

        return true;
    }

    private void PreTickModules()
    {
        if (_state != ApplicationState.Running) {
            return;
        }

        IterateOverModules<IPreTickableModule>("PreTick", module => module.PreTick());
    }

    private void TickModules()
    {
        if (_state != ApplicationState.Running) {
            return;
        }

        GetModule<IEventBus>().Flush();

        IterateOverModules<ITickableModule>("Tick", module => module.Tick());
    }

    private void FixedTickModules()
    {
        if (_state != ApplicationState.Running) {
            return;
        }

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
        if (_state != ApplicationState.Running) {
            return;
        }

        IterateOverModules<IPostTickableModule>("PostTick", module => module.PostTick());
    }

    private void PreRenderModules()
    {
        if (_state != ApplicationState.Running) {
            return;
        }

        IterateOverModules<IPreRenderableModule>("PreRender", module => module.PreRender());
    }

    private void RenderModules()
    {
        if (_state != ApplicationState.Running) {
            return;
        }

        IterateOverModules<IRenderableModule>("Render", module => module.Render());
    }

    private void PostRenderModules()
    {
        if (_state != ApplicationState.Running) {
            return;
        }

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
                TracyClient.Zone(zoneName + "::" + module.GetType().Name, callback, cast);
            }
        }
    }

    public void EnterPlayMode()
    {
        if (_state != ApplicationState.Running) {
            return;
        }

        IterateOverModules<IEnterPlayModeModule>("EnterPlayMode", module => module.EnterPlayMode());

        _isInPlayMode = true;
    }

    public void ExitPlayMode()
    {
        if (_state != ApplicationState.Running) {
            return;
        }

        IterateOverModules<IExitPlayModeModule>("ExitPlayMode", module => module.ExitPlayMode());

        _isInPlayMode = false;
    }

    protected virtual void RegisterModules()
    {
        AddModule(_platform);
        AddModule(new LogModule());
        AddModule(new EventBus());
        AddModule(new ContentModule(GetModule<ILogModule>()));
        AddModule(new GraphicsModule(this, _platform, GetModule<IEventBus>(), _renderSystem, GetModule<ILogModule>(), GetModule<IContentModule>()));
        AddModule(new InputModule(GetModule<ILogModule>(), _platform));
        AddModule(new PhysicsModule(GetModule<ILogModule>(), GetModule<IEventBus>()));
        AddModule(new AudioModule(GetModule<ILogModule>(), GetModule<IContentModule>()));
        AddModule(new UIModule(GetModule<ILogModule>(), GetModule<IContentModule>(), GetModule<IRendererModule>(), GetModule<IInputModule>()));
    }

    public void Run()
    {
        if (_state != ApplicationState.Running && _state != ApplicationState.Initialized) {
            throw new Exception("Application has not been initialized");
        }

        ChangeState(ApplicationState.Running);

        Time.FrameTimer?.Start();

        while (_state is ApplicationState.Running or ApplicationState.HotReloading) {
            RunFrame();
        }
    }

    public virtual void RunFrame()
    {
        Time.FrameTimer?.Update();

        Tick();
        Render();

        TracyClient.FrameMark();
    }

    public virtual void Tick()
    {
        PreTickModules();
        FixedTickModules();
        TickModules();
        PostTickModules();
    }

    public virtual void Render()
    {
        PreRenderModules();
        RenderModules();
        PostRenderModules();
    }

    public virtual void Shutdown()
    {
        if (_state == ApplicationState.TearingDown) {
            return;
        }

        _systemLogger?.LogInformation("Shutdown requested");

        ChangeState(ApplicationState.TearingDown);
        ShutdownModules();
    }

    public void AddModule(IModule module)
    {
        _modules.Add(module);
    }

    public void ChangeState(ApplicationState newState)
    {
        _state = newState;
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
