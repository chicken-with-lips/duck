using Arch.Core;
using Arch.Core.Utils;
using Duck.Content;
using Duck.Graphics.Content.SourceAssetImporter;
using Duck.Graphics.Device;
using Duck.Logging;
using Duck.Platform;
using Duck.ServiceBus;
using Silk.NET.Maths;

namespace Duck.Graphics;

public class GraphicsModule : GraphicsModuleBase,
    IInitializableModule,
    IPreRenderableModule,
    IPostRenderableModule,
    IHotReloadAwareModule,
    IModuleCanBeInstanced
{
    #region Properties

    public override IGraphicsDevice GraphicsDevice => _renderSystem.GraphicsDevice;
    public override IRenderSystem RenderSystem => _renderSystem;
    public override View PrimaryView { get; set; }

    #endregion

    #region Members

    private readonly ILogger _logger;
    private readonly IPlatform _platform;
    private readonly IRenderSystem _renderSystem;
    private readonly IApplication _app;
    private readonly IContentModule _contentModule;

    private IWindow? _window;

    #endregion

    #region Methods

    public GraphicsModule(IApplication app, IPlatform platform, IEventBus eventBus, IRenderSystem renderSystem, ILogModule logModule, IContentModule contentModule)
        : base(eventBus)
    {
        _app = app;
        _contentModule = contentModule;
        _platform = platform;
        _renderSystem = renderSystem;

        _logger = logModule.CreateLogger("Graphics");
        _logger.LogInformation("Created graphics module.");
    }

    #endregion

    #region IRenderingModule

    public bool Init()
    {
        _logger.LogInformation("Initializing graphics module...");

        _window = _platform.CreateWindow();

        _renderSystem.Init(_app, _window);

        _contentModule.RegisterSourceAssetImporter(
            new FbxAssetImporter(
                _app.GetModule<IContentModule>().ContentRootDirectory,
                _renderSystem.FallbackMaterial.MakeSharedReference()
            )
        );

        PrimaryView = CreateView("Primary");
        PrimaryView.Position = Vector2D<int>.Zero;
        PrimaryView.Dimensions = new Vector2D<int>(1280, 1024);
        PrimaryView.Position = new Vector2D<int>(0, 0);
        PrimaryView.IsEnabled = true;

        return true;
    }

    public override void Tick()
    {
        ProcessWindowEvents();

        base.Tick();
    }

    public void PreRender()
    {
        _renderSystem.PreRender();
    }

    public void PostRender()
    {
        _renderSystem.PostRender();
    }

    public void BeginHotReload()
    {
        foreach (var view in Views) {
            view.Scene = null;
            view.Camera = EntityReference.Null;
        }

        foreach (var scene in Scenes) {
            UnloadSceneNow(scene);
        }

        Console.WriteLine("TODO: ComponentRegistry.Types.Clear());");
    }

    public void EndHotReload()
    {
    }

    public IModule CreateModuleInstance(IApplication app)
    {
        return new InstancedGraphicsModule(this, app);
    }

    private void ProcessWindowEvents()
    {
        if (null == _window) {
            return;
        }

        foreach (var windowEvent in _window.Events) {
            if (windowEvent is ResizeEvent resizeEvent) {
                foreach (var view in Views) {
                    ResizeViewToWindow(view);
                }
            }
        }
    }

    #endregion
}
