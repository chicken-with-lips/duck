using Duck;
using Duck.ModuleManagement;
using Duck.Platform;

namespace GameLauncher;

class GameModule : IInitializableModule, ITickModule, IPreTickModule, IPostTickModule, IFixedTickModule, IDisposable
{
    private readonly Application _app;
    private ExternalModuleManager? _gameModuleManager;

    public GameModule(string assemblyPath, Application app)
    {
        _app = app;
        _gameModuleManager = new ExternalModuleManager(assemblyPath);
        _gameModuleManager.Changed += OnAssemblyChanged;
    }

    public void Initialize(IApplication app, IInitializationContext context)
    {
        if (!context.WasHotReloaded) {
            _gameModuleManager?.Initialize();
        }

        if (_gameModuleManager?.Handle?.Module is IInitializableModule initializableModule) {
            initializableModule.Initialize(app, context);
        }
    }

    public void Tick(FrameTimer frameTimer)
    {
        if (_gameModuleManager?.Handle?.Module is ITickModule tickableModule) {
            tickableModule.Tick(_app.FrameTimer);
        }
    }

    public void PreTick(FrameTimer frameTimer)
    {
        if (_gameModuleManager?.Handle?.Module is IPreTickModule preTickModule) {
            preTickModule.PreTick(_app.FrameTimer);
        }
    }

    public void PostTick(FrameTimer frameTimer)
    {
        if (_gameModuleManager?.Handle?.Module is IPostTickModule postTickModule) {
            postTickModule.PostTick(_app.FrameTimer);
        }
    }

    public void FixedTick(FrameTimer frameTimer)
    {
        if (_gameModuleManager?.Handle?.Module is IFixedTickModule fixedTickModule) {
            fixedTickModule.FixedTick(_app.FrameTimer);
        }
    }

    private void OnAssemblyChanged(ExternalModuleManager manager)
    {
        _app.ScheduleHotReload(manager);
    }

    public void Dispose()
    {
        _gameModuleManager?.Dispose();
        _gameModuleManager = null;
    }
}
