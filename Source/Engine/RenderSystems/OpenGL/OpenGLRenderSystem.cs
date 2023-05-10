using Duck.Content;
using Duck.Graphics;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Duck.Logging;
using Duck.Platform;
using Duck.Platforms.Standard;
using Duck.RenderSystems.OpenGL.ContentLoader;

namespace Duck.RenderSystems.OpenGL;

public class OpenGLRenderSystem : IRenderSystem
{
    #region Properties

    public IGraphicsDevice GraphicsDevice => _graphicsDevice;
    public IAsset<ShaderProgram> FallbackShader => _fallbackShader;

    #endregion

    #region Members

    private IAsset<ShaderProgram> _fallbackShader;
    private IAsset<ShaderProgram> _debugShader;

    private OpenGLGraphicsDevice? _graphicsDevice;
    private ILogger? _logger;

    #endregion

    public void Init(IApplication app, IWindow window)
    {
        var contentModule = app.GetModule<IContentModule>();

        _logger = app.GetModule<ILogModule>().CreateLogger("OpenGLRenderSystem");

        _fallbackShader = contentModule.Database.Register(CreateDefaultShader(contentModule));
        _debugShader = contentModule.Database.Register(CreateDebugShader(contentModule));

        _graphicsDevice = new OpenGLGraphicsDevice(((StandardWindow)window).InternalWindow.GLContext, window);

        FragmentShaderLoader fragmentShaderLoader;
        
        contentModule
            .RegisterAssetLoader<FragmentShader, OpenGLFragmentShader>(fragmentShaderLoader = new FragmentShaderLoader(_graphicsDevice, _logger))
            .RegisterAssetLoader<VertexShader, OpenGLVertexShader>(new VertexShaderLoader(_graphicsDevice))
            .RegisterAssetLoader<ShaderProgram, OpenGLShaderProgram>(new ShaderProgramLoader(_graphicsDevice, contentModule))
            .RegisterAssetLoader<Texture2D, OpenGLTexture2D>(new Texture2DLoader(_graphicsDevice))
            .RegisterAssetLoader<StaticMesh, OpenGLStaticMesh>(new StaticMeshLoader(_graphicsDevice, contentModule));

        var loadedDebugShader = (OpenGLShaderProgram)contentModule.LoadImmediate(_debugShader.MakeSharedReference());
        var loadedFallbackShader = (OpenGLShaderProgram)contentModule.LoadImmediate(_fallbackShader.MakeSharedReference());

        fragmentShaderLoader.FallbackShader = loadedFallbackShader;
        
        _graphicsDevice.Init(loadedDebugShader);
    }

    public void PreRender()
    {
        _graphicsDevice.BeginFrame();
    }

    public void Render()
    {
        // todo: rendering tasks
    }

    public void PostRender()
    {
        _graphicsDevice.Render();
        _graphicsDevice.EndFrame();
    }

    private IAsset<ShaderProgram> CreateDefaultShader(IContentModule contentModule)
    {
        var fragShader = contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Shaders/fallback.fs"))));
        var vertShader = contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Shaders/fallback.vs"))));

        return new ShaderProgram(
            new AssetImportData(new Uri("memory://default.shader")),
            vertShader.MakeSharedReference(),
            fragShader.MakeSharedReference()
        );
    }

    private IAsset<ShaderProgram> CreateDebugShader(IContentModule contentModule)
    {
        var fragShader = contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Shaders/debug.fs"))));
        var vertShader = contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Shaders/debug.vs"))));

        return new ShaderProgram(
            new AssetImportData(new Uri("memory://debug.shader")),
            vertShader.MakeSharedReference(),
            fragShader.MakeSharedReference()
        );
    }
}
