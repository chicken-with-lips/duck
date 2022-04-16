using Duck.Logging;
using Duck.Platform;
using Silk.NET.OpenGL;

namespace Duck.Graphics;

public class GraphicsModule : IGraphicsModule, ITickableModule, IRenderableModule
{
    #region Members

    private readonly ILogger _logger;
    private readonly IApplication _app;
    private readonly IPlatform _platform;

    #endregion

    #region Methods

    public GraphicsModule(IApplication app, ILogModule logModule, IPlatform platform)
    {
        _app = app;
        _platform = platform;

        _logger = logModule.CreateLogger("Graphics");
        _logger.LogInformation("Initializing graphics module.");
    }

    #endregion

    #region IRenderingModule

    public void Tick()
    {
        ProcessWindowEvents();
    }

    public void Render()
    {
        /*var gl = GL.GetApi(_platform.Window?.);
            
        gl.Clear((uint)ClearBufferMask.ColorBufferBit);

        float[] vertices = {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f, 0.5f, 0.0f
        };

        var vbo = gl.GenBuffer();
        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        gl.BufferData<float>(GLEnum.ArrayBuffer, (nuint) vertices.Length, vertices, GLEnum.StaticDraw);

        var vertexShaderSource = @"
#version 330 core
layout (location = 0) in vec3 aPos;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
}";

        var fragmentShaderSource = @"
#version 330 core
out vec4 FragColor;

void main()
{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}
";

        var vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, vertexShaderSource);
        gl.CompileShader(vertexShader);
        
        var infoLog = gl.GetShaderInfoLog(vertexShader);
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            throw new ApplicationException($"Error compiling vertex shader {infoLog}");
        }

        var fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, fragmentShaderSource);
        gl.CompileShader(fragmentShader);

        infoLog = gl.GetShaderInfoLog(fragmentShader);
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            throw new ApplicationException($"Error compiling fragment shader {infoLog}");
        }

        var shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);
        gl.LinkProgram(shaderProgram);

        gl.GetProgram(shaderProgram, GLEnum.LinkStatus, out var status);
        if (status == 0) {
            Console.WriteLine($"Error linking shader {gl.GetProgramInfoLog(shaderProgram)}");
        }
        
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
        
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 12, null);
        gl.EnableVertexAttribArray(0);*/
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
