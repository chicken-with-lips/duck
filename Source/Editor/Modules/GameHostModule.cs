using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Duck.GameFramework;
using Duck.GameHost;
using Duck.Input;
using Duck.Logging;
using Duck.Platform;

namespace Editor.Modules;

public class GameHostModule : IInitializableModule,
    ITickableModule,
    IRenderableModule,
    IShutdownModule,
    IEnterPlayModeModule,
    IExitPlayModeModule
{
    public IApplication InstancedApplication => _instancedApplication;
    public bool IsLoaded { get; private set; }
    public bool IsBusy { get; private set; }

    private readonly ApplicationBase _application;
    private readonly string _projectDirectory;
    private readonly ApplicationBase _instancedApplication;
    private readonly ILogger _logger;

    private GameAssemblyLoadContext? _assemblyContext;
    private IGameClient? _hostedClient;
    private bool _isInPlayMode;

    public GameHostModule(ApplicationBase application, ILogModule logModule, string projectDirectory)
    {
        _logger = logModule.CreateLogger("GameHost");
        _logger.LogInformation("Created game host module.");

        _application = application;
        _projectDirectory = projectDirectory;
        _instancedApplication = (ApplicationBase)application.CreateProxy(false);
    }

    public bool Init()
    {
        _logger.LogInformation("Initializing game host module...");

        if (!LoadAndInitialize()) {
            throw new Exception("Failed to initialize game client");
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Shutdown()
    {
        ExitPlayMode();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Tick()
    {
        AssertContextIsReady();

        _instancedApplication.Tick();
        _hostedClient?.Tick();

        if (_application.GetModule<IInputModule>().WasMouseButtonDown(1) && !IsBusy) {
            Reload();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Render()
    {
        AssertContextIsReady();

        _instancedApplication.Render();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void EnterPlayMode()
    {
        AssertContextIsReady();

        _instancedApplication.EnterPlayMode();
        _hostedClient?.EnterPlayMode();
    }

    public T GetModule<T>() where T : IModule
    {
        return _instancedApplication.GetModule<T>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void ExitPlayMode()
    {
        AssertContextIsReady();

        _hostedClient?.ExitPlayMode();
        _instancedApplication.ExitPlayMode();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool Load()
    {
        if (IsLoaded) {
            _logger.LogError("Client has already been loaded.");
            return false;
        }

        if (IsBusy) {
            _logger.LogError("Client is busy and cannot be loaded");
            return false;
        }

        IsBusy = true;

        _assemblyContext = new GameAssemblyLoadContext("Editor", true);

        var assemblies = new[] {
            "Duck.dll",
            // "Duck.CoreInterfaces.dll",
            "Duck.Ecs.dll",
            "Duck.GameFramework.dll",
            "Duck.Input.dll",
            "Duck.Scene.dll",
            "Duck.ServiceBus.dll",
            "Duck.ServiceBusInterfaces.dll",
        };

        foreach (var assembly in assemblies) {
            // _logger.LogDebug("Loading {0} in to context.", assembly);

            // _assemblyContext.LoadFromAssemblyPath(Path.Combine(directory, assembly));
        }

        var gameDll = Path.Combine(_projectDirectory, "Binaries", "net8.0", "Game.dll");

        using (_assemblyContext?.EnterContextualReflection()) {
            using (var stream = File.OpenRead(gameDll)) {
                var assembly = _assemblyContext?.LoadFromStream(stream);

                var clientType = assembly?.GetType("Game.GameClient");
                var x = Activator.CreateInstance(clientType);
                _hostedClient = x as IGameClient;
            }
        }

        IsBusy = false;
        IsLoaded = true;

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool LoadAndInitialize()
    {
        return Load()
               && Initialize(false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool Unload()
    {
        if (!IsLoaded) {
            _logger.LogError("Client cannot be unloaded because it was never loaded.");
            return false;
        }

        if (IsBusy) {
            _logger.LogError("Client is busy and cannot be unloaded");
            return false;
        }

        IsBusy = true;
        IsLoaded = false;

        var hotReloadContext = DoUnload(out var assemblyRef);

        var unloadTimer = new Stopwatch();
        unloadTimer.Start();

        while (assemblyRef.IsAlive) {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (unloadTimer.Elapsed.TotalSeconds > 1) {
                _logger.LogError("Waiting for game context to shutdown...");
                unloadTimer.Restart();
            }
        }

        IsBusy = false;

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IHotReloadContext DoUnload(out WeakReference contextRef)
    {
        contextRef = new WeakReference(_assemblyContext, true);

        var hotReloadContext = _application.BeginHotReload();

        _hostedClient = null;
        _assemblyContext?.Unload();
        _assemblyContext = null;

        return hotReloadContext;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool Reload()
    {
        return Unload()
               && Load()
               && Initialize(true);
    }

    private void AssertContextIsReady()
    {
        if (!IsLoaded || IsBusy) {
            throw new Exception("Client is not loaded or is busy");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool Initialize(bool isHotReload)
    {
        AssertContextIsReady();

        using (_assemblyContext?.EnterContextualReflection()) {
            var type = Type.GetType("Duck.GameFramework.GameClient.GameClientInitializationContext, Duck.GameFramework");
            var context = (IGameClientInitializationContext)Activator.CreateInstance(type, new object[] {
                _instancedApplication,
                isHotReload
            });

            _instancedApplication.Initialize();
            _hostedClient?.Initialize(context);
        }

        _instancedApplication.ChangeState(ApplicationState.Running);

        return true;
    }
}

class GameAssemblyLoadContext : AssemblyLoadContext
{
    public GameAssemblyLoadContext(string? name, bool isCollectible = false) : base(name, isCollectible)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.FullName.StartsWith("Duck.")) {
            return null;
        }

        Console.WriteLine("EditorClientAssemblyLoadContext.Load: " + assemblyName.FullName);

        return null;
        // string file = "/home/jolly_samurai/Projects/chicken-with-lips/infectic/Code/bin/Debug/net8.0/" + assemblyName.Name + ".dll";
        string file = "/media/jolly_samurai/Data/Projects/chicken-with-lips/Duck/Build/Debug/net9.0/" + assemblyName.Name + ".dll";
        Console.WriteLine(file);
        if (File.Exists(file)) {
            return LoadFromAssemblyPath(file);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        Console.WriteLine("EditorClientAssemblyLoadContext.LoadUnmanagedDll: " + unmanagedDllName);

        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}
