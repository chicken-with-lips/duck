using System.Diagnostics;
using Duck.Content;
using Duck.Graphics;
using Duck.Graphics.Device;
using Duck.Graphics.Materials;
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

    public IGraphicsDevice GraphicsDevice {
        get {
            Debug.Assert(_graphicsDevice != null, "GraphicsDevice has not been initialized");
            return _graphicsDevice;
        }
    }

    public IAsset<ShaderProgram> FallbackShader {
        get {
            Debug.Assert(_fallbackShader != null, "Fallback shader has not been initialized");
            return _fallbackShader;
        }
    }

    public IAsset<Material> FallbackMaterial {
        get {
            Debug.Assert(_fallbackMaterial != null, "Fallback material has not been initialized");
            return _fallbackMaterial;
        }
    }

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

        // FIXME: direct reference to standard window
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
        var fragShader = contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Builtin/Shaders/fallback.frag"))));
        var vertShader = contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Builtin/Shaders/fallback.vert"))));

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
        var fragShader = contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Builtin/Shaders/debug.frag"))));
        var vertShader = contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Builtin/Shaders/debug.vert"))));

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
