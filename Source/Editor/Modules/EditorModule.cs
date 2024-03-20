using System;
using ADyn.Components;
using Duck.Content;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Graphics.Systems;
using Duck.Logging;
using Duck.Platform;
using Duck.Ui;
using Editor.Ui;

namespace Editor.Modules;

public class EditorModule : IInitializableModule, IPostInitializableModule
{
    private readonly ILogger _logger;
    private readonly IApplication _application;
    private readonly IRendererModule _rendererModule;

    private View _editorView;
    private IScene _editorScene;
    private readonly IContentModule _contentModule;

    public EditorModule(IApplication application, ILogModule logModule, IRendererModule rendererModule, IContentModule contentModule, string projectDirectory)
    {
        _logger = logModule.CreateLogger("Editor");
        _logger.LogInformation("Created editor module.");

        _application = application;
        _rendererModule = rendererModule;
        _contentModule = contentModule;
    }

    public bool Init()
    {
        _logger.LogInformation("Initializing editor module...");

        return true;
    }

    public void PostInit()
    {
        _contentModule.Database.Register(new Font(new AssetImportData(new Uri("file:///Builtin/Fonts/Manrope/Medium.arfont"))));

        _editorScene = _rendererModule.CreateScene("Editor.SceneWindow");
        _editorScene.IsActive = true;

        _editorScene.SystemRoot.SimulationGroup
            .Add(new SceneEditorWindow(_editorScene.World, _editorScene, _application.GetModule<IUIModule>(), _application.GetModule<IContentModule>(), _application, _application.GetModule<GameHostModule>()));

        _editorScene.SystemRoot.PresentationGroup
            .Add(new RenderSceneSystem(_editorScene.World, _application.GetModule<IRendererModule>().GraphicsDevice));

        var world = _editorScene.World;

        var cameraEntity = world.Create(
            new CameraComponent(),
            new Position(),
            new Orientation()
        );

        world.Create(
            new SceneEditorWindowComponent()
        );

        _editorView = _rendererModule.CreateView("Editor.SceneWindow");
        _editorView.IsEnabled = true;
        _editorView.Camera = _editorScene.World.Reference(cameraEntity);
        _editorView.Scene = _editorScene;
    }
}
