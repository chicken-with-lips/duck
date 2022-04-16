using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Duck;
using Duck.GameFramework;
using Duck.GameHost;
using Duck.Logging;

namespace Editor.Host;

public class EditorClientHost
{
    #region Properties

    public bool IsLoaded { get; private set; }
    public bool IsBusy { get; private set; }

    #endregion

    #region Members

    private readonly ApplicationBase _application;
    private readonly ILogger _logger;
    private EditorClientAssemblyLoadContext? _assemblyContext;
    private IGameClient? _hostedClient;

    #endregion

    #region Methods

    public EditorClientHost(ApplicationBase application, ILogger logger)
    {
        _application = application;
        _logger = logger;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Load()
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

        _assemblyContext = new EditorClientAssemblyLoadContext("Editor", true);

        var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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

        // var gameDll = Path.Combine(directory, "Game.dll");
        var gameDll = "/home/jolly_samurai/Projects/chicken-with-lips/duck/Build/Debug/net6.0/Game/net6.0/Game.dll";


        using (_assemblyContext?.EnterContextualReflection()) {
            using (var stream = File.OpenRead(gameDll)) {
                var assembly = _assemblyContext?.LoadFromStream(stream);

                // var clientTypes = assembly
                //     .GetTypes()
                //     .Where(type => !type.IsAbstract && typeof(IGameClient).IsAssignableFrom(type))
                //     .ToArray();
                //
                // if (clientTypes.Length != 1) {
                //     _logger.LogError("There must be exactly one IClient defined.");
                //     return false;
                // }

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
    public bool LoadAndInitialize()
    {
        return Load()
               && Initialize(false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Unload()
    {
        if (!IsLoaded) {
            _logger.LogError("Client cannot be unloaded because it was never loaded.");
            return false;
        }

        if (IsBusy) {
            _logger.LogError("Client is busy and cannot be unloaded");
            return false;
        }

        // IsBusy = true;
        IsLoaded = false;

        var hotReloadContext = DoUnload(out var assemblyRef);

        var unloadTimer = new Stopwatch();
        unloadTimer.Start();

        while (assemblyRef.IsAlive) {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (unloadTimer.Elapsed.TotalSeconds > 0.1) {
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
    public bool Reload()
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
    public bool Initialize(bool isHotReload)
    {
        AssertContextIsReady();

        using (_assemblyContext?.EnterContextualReflection()) {
            var type = Type.GetType("Duck.GameFramework.GameClient.GameClientInitializationContext, Duck.GameFramework");
            var context = (IGameClientInitializationContext)Activator.CreateInstance(type, new object[] {
                _application,
                isHotReload
            });

            _hostedClient?.Initialize(context);
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Tick()
    {
        AssertContextIsReady();

        _hostedClient?.Tick();
    }

    #endregion
}
