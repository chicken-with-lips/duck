using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Graphics.OpenGL.ContentLoader;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Duck.Platform;
using Silk.NET.Core.Contexts;

namespace Duck.Graphics.OpenGL;

public class OpenGLPlatform : IPlatform
{
    #region Properties

    public IWindow[] Windows => _windows.ToArray();

    #endregion
    
    #region Members

    private readonly IApplication _app;
    private readonly List<IWindow> _windows = new();

    private IGLContext? _glContext;
    private bool _hasInitializedContent = false;

    #endregion

    #region Methods

    public OpenGLPlatform(IApplication app)
    {
        _app = app;
    }

    public IWindow CreateWindow()
    {
        var window = new OpenGLWindow(
            Configuration.Default,
            _glContext
        );

        if (_windows.Count == 0) {
            _glContext = window.GLContext;
        }

        _windows.Add(window);

        return window;
    }

    public IGraphicsDevice CreateGraphicsDevice()
    {
        if (_windows.Count == 0 || null == _glContext) {
            throw new Exception("FIXME: a window must be created first");
        }

        var device = new OpenGLGraphicsDevice(_glContext);
        var content = _app.GetModule<IContentModule>();

        content
            .RegisterAssetLoader<FragmentShader, OpenGLFragmentShader>(new FragmentShaderLoader(device))
            .RegisterAssetLoader<VertexShader, OpenGLVertexShader>(new VertexShaderLoader(device))
            .RegisterAssetLoader<ShaderProgram, OpenGLShaderProgram>(new ShaderProgramLoader(device, content))
            .RegisterAssetLoader<Texture2D, OpenGLTexture2D>(new Texture2DLoader(device))
            .RegisterAssetLoader<StaticMesh, OpenGLStaticMesh>(new StaticMeshLoader(device, content));

        return device;
    }

    public IFrameTimer CreateFrameTimer()
    {
        return new OpenGLFrameTimer();
    }

    public void Tick()
    {
        foreach (var window in _windows) {
            window.Update();
        }
    }

    public void PostTick()
    {
        foreach (var window in _windows) {
            window.ClearEvents();
        }
    }

    #endregion
}
