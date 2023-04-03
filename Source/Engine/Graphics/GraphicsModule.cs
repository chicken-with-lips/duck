using Duck.Content;
using Duck.Graphics.Content.SourceAssetImporter;
using Duck.Graphics.Device;
using Duck.Logging;
using Duck.Platform;

namespace Duck.Graphics;

public class GraphicsModule : IGraphicsModule,
    IInitializableModule,
    ITickableModule,
    IPreRenderableModule,
    IRenderableModule,
    IPostRenderableModule
{
    #region Properties

    public IGraphicsDevice? GraphicsDevice => _renderSystem.GraphicsDevice;
    
    #endregion
    
    #region Members

    private readonly ILogger _logger;
    private readonly IPlatform _platform;
    private readonly IRenderSystem _renderSystem;
    private readonly IApplication _app;
    private readonly IContentModule _contentModule;

    #endregion

    #region Methods

    public GraphicsModule(IApplication app, IPlatform platform, IRenderSystem renderSystem, ILogModule logModule, IContentModule contentModule)
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

        var window = _platform.CreateWindow();

        _renderSystem.Init(_app, window);

        _contentModule.RegisterSourceAssetImporter(
            new FbxAssetImporter(_renderSystem.DefaultShader.MakeSharedReference())
        );

        return true;
    }

    public void Tick()
    {
        ProcessWindowEvents();
    }

    public void PreRender()
    {
        _renderSystem.PreRender();
    }

    public void Render()
    {
        _renderSystem.Render();
    }

    public void PostRender()
    {
        _renderSystem.PostRender();
    }

    private void ProcessWindowEvents()
    {
        // foreach (var windowEvent in _nativeWindow.Events) {
        // if (windowEvent is NativeWindow.ResizeEvent resizeEvent) {
        // _renderingWindow?.Resize(resizeEvent.NewWidth, resizeEvent.NewHeight);
        // }
        // }
    }

    #endregion
}
