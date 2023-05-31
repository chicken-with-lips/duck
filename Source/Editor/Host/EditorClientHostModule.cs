using System;
using System.Diagnostics;
using Arch.Core.Extensions;
using Duck;
using Duck.Content;
using Duck.GameFramework;
using Duck.Input;
using Duck.Logging;
using Duck.Physics;
using Duck.Renderer;
using Duck.Renderer.Components;
using Duck.Renderer.Events;
using Duck.ServiceBus;
using Duck.Ui;
using Duck.Ui.Assets;
using Duck.Ui.Components;
using Duck.Ui.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;

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
        var sceneWindowInterface = _contentModule.Import<UserInterface>("Editor/UI/Windows/SceneWindow.rml");

        _rendererModule.GameView.IsEnabled = false;

        _eventBus.AddListener<SceneWasCreated>(ev => {
            var scene = ev.Scene;
            var world = scene.World;
            var composition = scene.SystemRoot;

            composition.SimulationGroup
                .Add(new ContextLoadSystem(world, GetModule<UiModule>()))
                .Add(new UserInterfaceLoadSystem(world, GetModule<IContentModule>(), GetModule<UiModule>()))
                .Add(new UserInterfaceTickSystem(world));

            composition.LateSimulationGroup
                .Add(new ContextSyncSystem(world, GetModule<UiModule>()));

            composition.PresentationGroup
                .Add(new UserInterfaceRenderSystem(world, GetModule<UiModule>()));
        });

        _editorScene = _rendererModule.CreateScene("Editor.SceneWindow");
        _editorScene.IsActive = true;

        var world = _editorScene.World;

        var cameraEntity = world.Create<CameraComponent, TransformComponent>();

        var sceneWindowEntity = world.Create();
        sceneWindowEntity.AddOrGet(new ContextComponent() {
            Name = "Editor.SceneWindow",
            ShouldReceiveInput = true
        });
        sceneWindowEntity.AddOrGet(new UserInterfaceComponent() {
            ContextName = "Editor.SceneWindow",
            Interface = sceneWindowInterface.MakeUniqueReference(),
        });

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
