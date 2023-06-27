using System;
using Arch.Core.Extensions;
using Duck;
using Duck.Content;
using Duck.GameFramework;
using Duck.Logging;
using Duck.Renderer;
using Duck.Renderer.Components;
using Duck.ServiceBus;
using Duck.Ui;

namespace Editor.Host;

public class EditorClientHostModule : IInitializableModule, IPostInitializableModule, ITickableModule
{
    private readonly ILogger _logger;
    private readonly EditorClientHost _clientHost;
    private readonly IApplication _application;
    private readonly IRendererModule _rendererModule;
    private readonly IContentModule _contentModule;
    private readonly IEventBus _eventBus;

    private View? _editorView;
    private IScene? _editorScene;

    public EditorClientHostModule(ApplicationBase application, ILogModule logModule, string projectDirectory)
    {
        _logger = logModule.CreateLogger("GameHost");
        _logger.LogInformation("Created game host module.");

        _application = application;
        _rendererModule = GetModule<IRendererModule>();
        _contentModule = GetModule<IContentModule>();
        _eventBus = GetModule<IEventBus>();
        _clientHost = new EditorClientHost(application, _logger, projectDirectory);
    }

    public bool Init()
    {
        _logger.LogInformation("Initializing game host module...");

        if (!_clientHost.LoadAndInitialize()) {
            throw new Exception("Failed to initialize game client");
        }

        return true;
    }

    public void PostInit()
    {
        _rendererModule.GameView.IsEnabled = false;

        _editorScene = _rendererModule.CreateScene("Editor.SceneWindow");
        _editorScene.IsActive = true;

        var world = _editorScene.World;

        var cameraEntity = world.Create<CameraComponent, TransformComponent>();

        // world.Create(
        // new UserInterfaceComponent()
        // );

        _editorView = _rendererModule.CreateView("Editor.SceneWindow");
        _editorView.IsEnabled = true;
        _editorView.Camera = cameraEntity.Reference();
        _editorView.Scene = new WeakReference<IScene>(_editorScene);
    }

    public void Tick()
    {
        // _clientHost.Tick();
    }

    private T GetModule<T>()
        where T : IModule
    {
        return _application.GetModule<T>();
    }
}
