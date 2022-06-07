using Duck.Content;
using Duck.Graphics.Content.SourceAssetImporter;
using Duck.Graphics.Device;
using Duck.Graphics.OpenGL;
using Duck.Graphics.Shaders;
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

    public IGraphicsDevice GraphicsDevice => _graphicsDevice;

    #endregion

    #region Members

    private readonly ILogger _logger;
    private readonly OpenGLPlatform _platform;
    private readonly IContentModule _contentModule;

    private IGraphicsDevice? _graphicsDevice;
    private IAsset<ShaderProgram> _defaultShader;

    #endregion

    #region Methods

    public GraphicsModule(IApplication app, IPlatform platform, ILogModule logModule, IContentModule contentModule)
    {
        _contentModule = contentModule;
        _platform = (OpenGLPlatform)platform;

        _logger = logModule.CreateLogger("Graphics");
        _logger.LogInformation("Created graphics module.");
    }

    #endregion

    #region IRenderingModule

    public bool Init()
    {
        _logger.LogInformation("Initializing graphics module...");

        _platform.CreateWindow();
        _graphicsDevice = _platform.CreateGraphicsDevice();
        _defaultShader = _contentModule.Database.Register(CreateDefaultShader());

        _contentModule.RegisterSourceAssetImporter(new FbxAssetImporter(_defaultShader.MakeReference()));

        return true;
    }

    public void Tick()
    {
        ProcessWindowEvents();
    }

    public void PreRender()
    {
        _graphicsDevice?.BeginFrame();
    }

    public void Render()
    {
        _graphicsDevice?.Render();
    }

    public void PostRender()
    {
        _graphicsDevice?.EndFrame();
    }

    private void ProcessWindowEvents()
    {
        // foreach (var windowEvent in _nativeWindow.Events) {
        // if (windowEvent is NativeWindow.ResizeEvent resizeEvent) {
        // _renderingWindow?.Resize(resizeEvent.NewWidth, resizeEvent.NewHeight);
        // }
        // }
    }

    private IAsset<ShaderProgram> CreateDefaultShader()
    {
        var fragShader = _contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Shaders/shader.fs"))));
        var vertShader = _contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Shaders/shader.vs"))));

        return new ShaderProgram(
            new AssetImportData(new Uri("memory://generated")),
            vertShader.MakeReference(),
            fragShader.MakeReference()
        );
    }

    #endregion
}
