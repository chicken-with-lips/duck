using Duck.Scene;
using Duck.Platform;
using Duck.Platform.Logging;
using Godot;
using Logger = Duck.Platform.Logging.Logger;
using Vector3 = System.Numerics.Vector3;

namespace Duck.RenderSystem.Godot;

public class GodotPlatform : IPlatform
{
    public IWindow PrimaryWindow => _windows.First(window => window.IsPrimary);
    public IView PrimaryView => _viewports.First(viewport => viewport.IsPrimary);
    
    public SceneTree SceneTree => (SceneTree) Engine.GetMainLoop();

    private readonly List<IWindow> _windows = new();
    private readonly List<GodotView> _viewports = new();

    private IntPtr _godotInstancePtr = IntPtr.Zero;
    private GodotInstance? _godotInstance;
    private Logger? _logger;

    public void Initialize(IApplication app)
    {
        _logger = app.GetModule<LogModule>().CreateLogger("Platform");
        _logger.LogInformation("Initializing godot platform");

        string[] godotArgs = new[] {
            "driver",
            "--path", "/home/iain/Projects/chicken-with-lips/SolarEcho/Assets"
        };

        _godotInstancePtr = LibGodot.libgodot_create_godot_instance(
            godotArgs.Length,
            godotArgs,
            LibGodot.InitCallback
        );

        if (_godotInstancePtr == IntPtr.Zero) {
            throw new InvalidOperationException("Could not initialize platform");
        }

        if (!LibGodot.CallGodotInstanceStart(_godotInstancePtr)) {
            LibGodot.libgodot_destroy_godot_instance(_godotInstancePtr);

            throw new InvalidOperationException("Could not start platform bridge");
        }

        Engine.Singleton.PrintErrorMessages = true;
        Engine.Singleton.MaxFps = 0;

        _godotInstance = LibGodot.GetGodotInstanceFromPtr(_godotInstancePtr);

        if (_godotInstance == null) {
            LibGodot.libgodot_destroy_godot_instance(_godotInstancePtr);

            throw new InvalidOperationException("Failed to get bridge from pointer");
        }

        var sceneTree = Engine.GetMainLoop() as SceneTree;

        if (sceneTree == null) {
            LibGodot.libgodot_destroy_godot_instance(_godotInstancePtr);

            throw new InvalidOperationException("Failed to get scene tree");
        }

        _windows.Add(
            new GodotWindow(0, true)
        );

        _viewports.Add(
            new GodotView(
                app.GetModule<GodotRenderModule>(),
                sceneTree.Root.GetViewportRid(),
                "Primary",
                true,
                PrimaryWindow.Dimensions
            )
        );
    }

    public void Shutdown()
    {
        if (_godotInstancePtr != IntPtr.Zero) {
            LibGodot.libgodot_destroy_godot_instance(_godotInstancePtr);
            _godotInstancePtr = IntPtr.Zero;
        }
    }

    public bool Update()
    {
        foreach (var viewport in _viewports) {
            viewport.Tick();
        }

        // EditorInterface.Singleton.GetResourceFilesystem().Scan();
        
        return !_godotInstance?.Iteration() ?? false;
    }

    public FrameTimer CreateFrameTimer()
    {
        return new FrameTimer();
    }

    public IWindow CreateWindow(in WindowConfiguration? config = null)
    {
        throw new NotImplementedException();
    }
}