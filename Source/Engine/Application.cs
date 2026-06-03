using System.Collections.Concurrent;
using System.Reflection;
using Duck.ModuleManagement;
using Duck.Platform;
using Duck.Platform.Logging;
using Duck.Scene;
using Duck.Serialization;
using Schedulers;

namespace Duck;

public interface IApplication
{
    JobScheduler Scheduler { get; }
    IPlatform Platform { get; }

    T GetModule<T>() where T : IModule;
    T[] GetModules<T>() where T : IModule;
}

public delegate IPlatform CreatePlatformDelegate(Application application);

public class Application : IApplication
{
    public Application(CreatePlatformDelegate createPlatform)
    {
        AddModule(new LogModule());
        AddModule(new SceneModule(CreateLogger("Scene")));

        _logger = CreateLogger("Engine");

        Platform = createPlatform(this);
        FrameTimer = Platform.CreateFrameTimer();

        _frameAccumulator = new FrameAccumulator(FrameTimer) {
            TargetFrameRate = 60,
        };

        _fixedTickAccumulator = new FrameAccumulator(FrameTimer) {
            TargetFrameRate = 1,
        };

        _scheduler = new JobScheduler(new JobScheduler.Config {
                ThreadPrefixName = "Duck",
                ThreadCount = 0, // determine automatically
                MaxExpectedConcurrentJobs = 64,
                StrictAllocationMode = false,
            }
        );

        _logger.LogInformation("Initialized job scheduler:");
        _logger.LogInformation("...thread count: {0}", _scheduler.ThreadCount);
    }

    public T GetModule<T>() where T : IModule
    {
        return (T)_modules.First(module => module is T);
    }

    public T[] GetModules<T>() where T : IModule
    {
        return _modules
            .OfType<T>()
            .ToArray();
    }

    public Logger CreateLogger(string categoryName)
    {
        return GetModule<LogModule>()
            .CreateLogger(categoryName);
    }

    public void Initialize()
    {
        if (_state != ApplicationState.Uninitialized) {
            throw new Exception("Application must be in the uninitialized state");
        }

        ChangeState(ApplicationState.Initializing);

        var context = new InitializationContext(false);

        Platform.Initialize(this);

        IterateModules<IInitializableModule>(module => module.Initialize(this, context));

        Serializer.Init();

        ChangeState(ApplicationState.Initialized);
    }

    public void AddModule(IModule module)
    {
        if (!_modules.Contains(module)) {
            _modules.Add(module);
        }
    }

    private void IterateModules<T>(Action<T> action, bool forceSingleThreaded = false) where T : IModule
    {
        var modules = GetModules<T>();

        foreach (var t in modules) {
            if (t is ISingleThreadedModule || forceSingleThreaded) {
                action(t);
            } else {
                _iterationJobHandles.Add(
                    Scheduler.Schedule(new IterateJob<T>(action, t))
                );
            }
        }

        Scheduler.Flush();
        JobHandle.CompleteAll(_iterationJobHandles);
        _iterationJobHandles.Clear();
    }

    public void RequestShutdown()
    {
        ChangeState(ApplicationState.ShuttingDown);
    }

    public void Run()
    {
        if (_state != ApplicationState.Initialized) {
            throw new Exception("Application must be in the initialized state");
        }

        ChangeState(ApplicationState.Running);

        FrameTimer.Start();

        while (_state is ApplicationState.Running or ApplicationState.HotReloading) {
            lock (_queuedHotReloadRequests) {
                if (!_queuedHotReloadRequests.IsEmpty) {
                    ProcessHotReload();

                    continue;
                }
            }

            FrameTimer.Update();

            if (_state == ApplicationState.Running) {
                _frameAccumulator.Update();
                _fixedTickAccumulator.Update();

                while (_frameAccumulator.Consume()) {
                    IterateModules<IPreTickModule>(m => m.PreTick(FrameTimer));
                    IterateModules<ITickModule>(m => m.Tick(FrameTimer));
                    IterateModules<IPostTickModule>(m => m.PostTick(FrameTimer));
                }

                while (_fixedTickAccumulator.Consume()) {
                    IterateModules<IFixedTickModule>(m => m.FixedTick(FrameTimer));
                }

                IterateModules<IPresentSingleThreadedModule>(m => m.Present(FrameTimer), true);
            }

            if (!Platform.Update()) {
                RequestShutdown();
            }
        }

        IterateModules<IShutdownModule>(m => m.Shutdown(this));

        Platform.Shutdown();

        _scheduler?.Dispose();
    }

    private void ChangeState(ApplicationState newState)
    {
        if (newState == _state) {
            return;
        }

        _logger.LogDebug($"Changed state from {_state} to {newState}");

        _state = newState;
    }

    public void ScheduleHotReload(ExternalModuleManager manager)
    {
        lock (_queuedHotReloadRequests) {
            if (!_queuedHotReloadRequests.Contains(manager)) {
                _queuedHotReloadRequests.Add(manager);
            }
        }

        _logger.LogDebug("Hot reload scheduled");
    }

    private void ProcessHotReload()
    {
        ChangeState(ApplicationState.HotReloading);

        if (!_isWaitingForUnload) {
            var instigators = _queuedHotReloadRequests
                .Where(m => m.Handle != null)
                .Select(m => {
                        var r = m.Reload();

                        return new HotReloadInstigator(r.Item1!, r.Item2);
                    }
                )
                .ToArray();


            GetModules<IHotReloadSubscriberModule>()
                .ForEach(m => m.BeginHotReload(instigators));
            Serializer.Clear();

            _isWaitingForUnload = true;
        }

        _queuedHotReloadRequests.ForEach(m => m.PumpUnload());

        if (_queuedHotReloadRequests.Count(m => !m.IsPendingUnloadComplete) > 0) {
            return;
        }

        _queuedHotReloadRequests.Clear();

        Serializer.Init();

        GetModules<IHotReloadSubscriberModule>()
            .ForEach(m => m.EndHotReload());
        GetModules<IInitializableModule>()
            .ForEach(m => m.Initialize(this, new InitializationContext(true)));

        ChangeState(ApplicationState.Running);

        _isWaitingForUnload = false;

        _logger.LogInformation("Hot reload finished.");
    }

    private readonly struct IterateJob<T> : IJob
    {
        private readonly Action<T> _action;
        private readonly T _module;

        public IterateJob(Action<T> action, T module)
        {
            _action = action;
            _module = module;
        }

        public void Execute()
        {
            _action.Invoke(_module);
        }
    }

    #region Properties

    public FrameTimer FrameTimer { get; }

    public JobScheduler Scheduler
    {
        get => _scheduler!;
    }

    public IPlatform Platform { get; }

    #endregion

    #region Members

    private ApplicationState _state = ApplicationState.Uninitialized;

    private readonly JobScheduler? _scheduler;
    private readonly Logger _logger;
    private readonly FrameAccumulator _frameAccumulator;
    private readonly FrameAccumulator _fixedTickAccumulator;

    private readonly List<IModule> _modules = [];
    private readonly ConcurrentBag<ExternalModuleManager> _queuedHotReloadRequests = [];

    private bool _isWaitingForUnload;

    private readonly List<JobHandle> _iterationJobHandles = new();

    #endregion
}

public enum ApplicationState
{
    Uninitialized,
    Initializing,
    Initialized,
    Running,
    HotReloading,
    ShuttingDown,
}

public class HotReloadInstigator
{
    public HotReloadInstigator(Assembly current, Assembly next)
    {
        Current = current;
        Next = next;
    }

    public Assembly Current { get; }
    public Assembly Next { get; }
}
