using Duck.Content;
using Duck.Renderer;
using Duck.Renderer.Device;
using Duck.Renderer.Materials;
using Duck.Renderer.Mesh;
using Duck.Renderer.Shaders;
using Duck.Renderer.Textures;
using Duck.Logging;
using Duck.Platform;
using Duck.Platforms.Standard;
using Duck.RenderSystems.OpenGL.ContentLoader;

namespace Duck.RenderSystems.OpenGL;

public class OpenGLRenderSystem : IRenderSystem
{
    #region Properties

    public IGraphicsDevice? GraphicsDevice => _graphicsDevice;
    public IAsset<ShaderProgram>? FallbackShader => _fallbackShader;
    public IAsset<Material>? FallbackMaterial => _fallbackMaterial;

    #endregion

    #region Members

    private IAsset<ShaderProgram>? _fallbackShader;
    private IAsset<Material>? _fallbackMaterial;
    private IAsset<ShaderProgram>? _debugShader;
    private IAsset<Material>? _debugMaterial;

    private OpenGLGraphicsDevice? _graphicsDevice;
    private ILogger? _logger;

    #endregion

    public void Init(IApplication app, IWindow window)
    {
        var contentModule = app.GetModule<IContentModule>();

        _logger = app.GetModule<ILogModule>().CreateLogger("OpenGLRenderSystem");

        _fallbackShader = contentModule.Database.Register(CreateFallbackShader(contentModule));
        _fallbackMaterial = contentModule.Database.Register(CreateFallbackMaterial(contentModule, _fallbackShader));
        _debugShader = contentModule.Database.Register(CreateDebugShader(contentModule));
        _debugMaterial = contentModule.Database.Register(CreateDebugMaterial(contentModule));

        _graphicsDevice = new OpenGLGraphicsDevice(((StandardWindow)window).InternalWindow.GLContext, window);

        FragmentShaderLoader fragmentShaderLoader;
        VertexShaderLoader vertexShaderLoader;

        contentModule
            .RegisterAssetLoader<FragmentShader, OpenGLFragmentShader>(fragmentShaderLoader = new FragmentShaderLoader(_graphicsDevice, _logger))
            .RegisterAssetLoader<VertexShader, OpenGLVertexShader>(vertexShaderLoader = new VertexShaderLoader(_graphicsDevice, _logger))
            .RegisterAssetLoader<ShaderProgram, OpenGLShaderProgram>(new ShaderProgramLoader(_graphicsDevice, contentModule))
            .RegisterAssetLoader<Material, OpenGLMaterial>(new MaterialLoader(contentModule))
            .RegisterAssetLoader<Texture2D, OpenGLTexture2D>(new Texture2DLoader(_graphicsDevice))
            .RegisterAssetLoader<StaticMesh, OpenGLStaticMesh>(new StaticMeshLoader(_graphicsDevice, contentModule));

        var loadedDebugShader = (OpenGLShaderProgram)contentModule.LoadImmediate(_debugShader.MakeSharedReference());
        var loadedDebugMaterial = (OpenGLMaterial)contentModule.LoadImmediate(_debugMaterial.MakeSharedReference());
        var loadedFallbackShader = (OpenGLShaderProgram)contentModule.LoadImmediate(_fallbackShader.MakeSharedReference());

        fragmentShaderLoader.FallbackShader = loadedFallbackShader;
        vertexShaderLoader.FallbackShader = loadedFallbackShader;

        _graphicsDevice.Init(loadedDebugMaterial);
    }

    public void PreRender()
    {
        _graphicsDevice?.BeginFrame();
    }

    public void Render()
    {
        // todo: rendering tasks
    }

    public void PostRender()
    {
        _graphicsDevice?.EndFrame();
    }

    private IAsset<ShaderProgram> CreateFallbackShader(IContentModule contentModule)
    {
        var fragShader = contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Shaders/fallback.frag"))));
        var vertShader = contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Shaders/fallback.vert"))));

        return new ShaderProgram(
            new AssetImportData(new Uri("memory://engine/fallback.shader")),
            vertShader.MakeSharedReference(),
            fragShader.MakeSharedReference()
        );
    }

    private IAsset<Material> CreateFallbackMaterial(IContentModule contentModule, IAsset<ShaderProgram> fallbackShader)
    {
        var material = new Material(
            new AssetImportData(new Uri("memory://engine/fallback.mat"))
        );

        material.Shader = fallbackShader.MakeSharedReference();

        return material;
    }

    private IAsset<ShaderProgram> CreateDebugShader(IContentModule contentModule)
    {
        var fragShader = contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Shaders/debug.frag"))));
        var vertShader = contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Shaders/debug.vert"))));

        return new ShaderProgram(
            new AssetImportData(new Uri("memory://engine/debug.shader")),
            vertShader.MakeSharedReference(),
            fragShader.MakeSharedReference()
        );
    }

    private IAsset<Material> CreateDebugMaterial(IContentModule contentModule)
    {
        var material = new Material(
            new AssetImportData(new Uri("memory://engine/debug.mat"))
        );
        material.Shader = _debugShader?.MakeSharedReference();

        return material;
    }
}
