using Duck.ModuleManagement;
using Duck.Platform.Logging;
using Duck.RenderSystem.Godot.Systems;
using Duck.Scene;
using Godot;
using Environment = System.Environment;
using Logger = Duck.Platform.Logging.Logger;

namespace Duck.RenderSystem.Godot;

public class GodotRenderModule : IInitializableModule, IShutdownModule, ISingleThreadedModule
{
    private readonly List<GodotSceneLink> _sceneLinks = new();
    private GodotPlatform _platform;
    private Logger _logger;
    private PackedScene _blankGodotScene;

    public GodotRenderModule(LogModule logModule)
    {
        _logger = logModule.CreateLogger("Render");
    }

    public void Initialize(IApplication app, IInitializationContext context)
    {
        if (context.WasHotReloaded) {
            return;
        }

        _platform = (GodotPlatform)app.Platform;

        _logger.LogInformation("Initializing godot render module" + Environment.NewLine +
                               "\t...video adapter: {1}" + Environment.NewLine +
                               "\t...driver: {0}" + Environment.NewLine +
                               "\t...method: {0}",
            RenderingServer.Singleton.GetRenderingDevice().GetDeviceName(),
            RenderingServer.Singleton.GetCurrentRenderingDriverName(),
            RenderingServer.Singleton.GetCurrentRenderingMethod()
        );

        app.GetModule<SceneModule>().SceneCreated += OnSceneCreated;

        _blankGodotScene = new PackedScene();
        _blankGodotScene.Pack(new Node3D());
    }

    public void Shutdown(IApplication app)
    {
        app.GetModule<SceneModule>().SceneCreated -= OnSceneCreated;

        _sceneLinks.Clear();
    }

    private GodotSceneLink GetSceneLink(IScene scene)
    {
        return _sceneLinks.First(x => x.Scene == scene);
    }

    private Mesh cube;
    public static StandardMaterial3D mat;
    private ImageTexture tex;

    private void OnSceneCreated(IScene scene)
    {
        scene.SystemRoot.LateSimulationGroup.Add(new CameraSystem(scene.World));

        var godotScene = (Node3D)_blankGodotScene.Instantiate();
        cube = ResourceLoader.Load<Mesh>("res://cube.obj");

        mat = new StandardMaterial3D();
        
        mat.AlbedoColor = new Color(1, 0, 0);

        var img = Image.LoadFromFile("/home/iain/Downloads/images.jpg");
        tex = ImageTexture.CreateFromImage(img);

        
        
        Console.WriteLine(OS.GetDataDir());
        Console.WriteLine(ResourceSaver.Save(img, "res://out.tres"));
        Console.WriteLine(ResourceSaver.Save(mat, "res://out2.tres"));
        Console.WriteLine(ResourceSaver.GetResourceIdForPath("res://out2.tres", true));

        mat.AlbedoTexture = tex;
        
        RenderingServer.CallOnRenderThread(Callable.From(() => {
            _platform.SceneTree.Root.AddChild(godotScene);
            _platform.SceneTree.CurrentScene = godotScene;

            var instanceId = RenderingServer.InstanceCreate();
            RenderingServer.InstanceSetScenario(instanceId, godotScene.GetWorld3D().Scenario);
            RenderingServer.InstanceSetBase(instanceId, cube.GetRid());
            RenderingServer.InstanceSetTransform(instanceId,
                new Transform3D(Basis.Identity, new Vector3(0, 0, -5)));
            RenderingServer.InstanceSetSurfaceOverrideMaterial(instanceId, 0, mat.GetRid());
            cubeRid = instanceId;
        }));

        _sceneLinks.Add(
            new GodotSceneLink(scene, godotScene)
        );
    }

    public static Rid cubeRid;
}