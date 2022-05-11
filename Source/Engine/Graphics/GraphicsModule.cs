using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Graphics.OpenGL;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Duck.Logging;
using Duck.Math;
using Duck.Platform;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using AttributeType = Duck.Graphics.Device.AttributeType;

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
    private readonly IApplication _app;
    private readonly OpenGLPlatform _platform;
    private readonly IContentModule _contentModule;

    private IGraphicsDevice _graphicsDevice;

    #endregion

    #region Methods

    public GraphicsModule(IApplication app, IPlatform platform, ILogModule logModule, IContentModule contentModule)
    {
        _app = app;
        _platform = platform as OpenGLPlatform;
        _contentModule = contentModule;

        _logger = logModule.CreateLogger("Graphics");
        _logger.LogInformation("Initializing graphics module.");
    }

    #endregion

    #region IRenderingModule

    // private uint shaderProgram;
    // private OpenGLShaderProgram _shaderProgram;
    // private OpenGLTexture2D _texture;
    // private OpenGLStaticMesh _cubeMesh;

    // private BufferObject<float> _vertexBuffer;

    // private BufferObject<uint> _indexBuffer;
    // private OpenGLVertexBuffer<float> _openGlVertexBuffer;
    // private OpenGLIndexBuffer<uint> _openGlIndexBuffer;
    // private VertexArrayObject<float, uint> _vertexArrayObject;

    // private float[] vertices = {
    //     // positions          // colors           // texture coords
    //     0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, // top right
    //     0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, // bottom right
    //     -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, // bottom left
    //     -0.5f, 0.5f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f // top left 
    // };
    //
    // private uint[] indices = {
    //     0, 1, 3, // first triangle
    //     1, 2, 3 // second triangle
    // };

    public bool Init()
    {
        _platform.CreateWindow();
        _graphicsDevice = _platform.CreateGraphicsDevice();

        var content = _app.GetModule<IContentModule>();


        // var fragShader = content.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Content/Shaders/shader.fs"))));
        // var vertShader = content.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Content/Shaders/shader.vs"))));
        // var program = content.Database.Register(new ShaderProgram(new AssetImportData(new Uri("memory://generated")), vertShader.MakeReference(), fragShader.MakeReference()));
        // var texture = content.Database.Register(new Texture2D(new AssetImportData(new Uri("file:///Content/PolygonPrototype/Textures/PolygonPrototype_Texture_Grid_08.png"))));


        // var vertices = new float[] {
        //     -0.5f, -0.5f, -0.5f,
        //     0.0f, 0.0f,
        //
        //     -0.5f, 0.5f, -0.5f,
        //     1.0f, 0.0f,
        //
        //     0.5f, 0.5f, -0.5f,
        //     1.0f, 1.0f,
        //
        //     0.5f, -0.5f, -0.5f,
        //     0.0f, 1.0f,
        //
        //     // Face +X
        //     0.5f, -0.5f, -0.5f,
        //     0.0f, 0.0f,
        //
        //     0.5f, 0.5f, -0.5f,
        //     1.0f, 0.0f,
        //
        //     0.5f, 0.5f, 0.5f,
        //     1.0f, 1.0f,
        //
        //     0.5f, -0.5f, 0.5f,
        //     0.0f, 1.0f,
        //
        //     // Face +Z
        //     -0.5f, -0.5f, 0.5f,
        //     0.0f, 0.0f,
        //
        //     0.5f, -0.5f, 0.5f,
        //     1.0f, 0.0f,
        //
        //     0.5f, 0.5f, 0.5f,
        //     1.0f, 1.0f,
        //
        //     -0.5f, 0.5f, 0.5f,
        //     0.0f, 1.0f,
        //
        //     // Face -X
        //     -0.5f, -0.5f, 0.5f,
        //     0.0f, 0.0f,
        //
        //     -0.5f, 0.5f, 0.5f,
        //     1.0f, 0.0f,
        //
        //     -0.5f, 0.5f, -0.5f,
        //     1.0f, 1.0f,
        //
        //     -0.5f, -0.5f, -0.5f,
        //     0.0f, 1.0f,
        //
        //     // Face -Y
        //     -0.5f, -0.5f, 0.5f,
        //     0.0f, 0.0f,
        //
        //     -0.5f, -0.5f, -0.5f,
        //     1.0f, 0.0f,
        //
        //     0.5f, -0.5f, -0.5f,
        //     1.0f, 1.0f,
        //
        //     0.5f, -0.5f, 0.5f,
        //     0.0f, 1.0f,
        //
        //     // Face +Y
        //     -0.5f, 0.5f, -0.5f,
        //     0.0f, 0.0f,
        //
        //     -0.5f, 0.5f, 0.5f,
        //     1.0f, 0.0f,
        //
        //     0.5f, 0.5f, 0.5f,
        //     1.0f, 1.0f,
        //
        //     0.5f, 0.5f, -0.5f,
        //     0.0f, 1.0f,
        // };
        //
        // var indices = new List<uint>();
        //
        // for (var it = 0; it < 6; it++) {
        //     var i = (ushort)(it * 4);
        //
        //     indices.Add(i);
        //     indices.Add((ushort)(i + 1));
        //     indices.Add((ushort)(i + 2));
        //     indices.Add((ushort)i);
        //     indices.Add((ushort)(i + 2));
        //     indices.Add((ushort)(i + 3));
        // }

        // var vertexBuffer = VertexBufferBuilder<float>.Create(BufferUsage.Static)
        //     .Attribute(VertexAttribute.Position, 0, AttributeType.Float3)
        //     .Attribute(VertexAttribute.Uv0, 0, AttributeType.Float2)
        //     .Build(_graphicsDevice);
        //
        // var indexBuffer = IndexBufferBuilder<uint>.Create(BufferUsage.Static)
        //     .Build(_graphicsDevice);
        //
        // _vertexArrayObject = _graphicsDevice.CreateVertexArrayObject(_openGlVertexBuffer, _openGlIndexBuffer);
        // _vertexArrayObject
        //     .Bind()
        //     .VertexAttribute(0, 3, DataType.Float, 8, 0)
        //     .VertexAttribute(1, 3, DataType.Float, 8, 3)
        //     .VertexAttribute(2, 2, DataType.Float, 8, 6);

        // var vertexBuffer = new VertexBufferDefinition<float>(BufferUsage.Static, vertices);
        // var indexBuffer = new IndexBufferDefinition<uint>(BufferUsage.Static, indices.ToArray());
        // var cubeAsset = content.Database.Register(
        //     new StaticMesh(
        //         new AssetImportData(new Uri("memory://generated")),
        //         new BufferObject<float>(vertices),
        //         new BufferObject<uint>(indices.ToArray()),
        //         program.MakeReference(),
        //         texture.MakeReference()
        //     )
        // );
        //
        // _cubeMesh = (OpenGLStaticMesh)_contentModule.LoadImmediate(cubeAsset);

        return true;
    }

    public void Tick()
    {
        ProcessWindowEvents();
    }

    public void PreRender()
    {
        _graphicsDevice.BeginFrame();
    }

    public unsafe void Render()
    {
        _graphicsDevice.Render();

        // float greenValue = (MathF.Sin((float)Time.Elapsed) / 2f) + 0.5f;

        // _cubeMesh.Render();

        // _vertexArrayObject.Bind();
        //
        // _gl.UseProgram(_shaderProgram.ProgramId);
        // _gl.BindTexture(TextureTarget.Texture2D, _texture.TextureId);
        //
        // Matrix4X4<float> trans = Matrix4X4<float>.Identity;
        // trans *= Matrix4X4.CreateFromAxisAngle(Vector3D<float>.UnitZ, MathHelper.ToRadians(Time.Elapsed));
        // // trans *= Matrix4X4.CreateScale(0.5f, 0.5f, 0.5f);
        // trans *= Matrix4X4.CreateTranslation(0.5f, -0.5f, 0f);
        //
        //
        // Matrix4X4<float> model = Matrix4X4<float>.Identity * Matrix4X4.CreateRotationX(Time.Elapsed * MathHelper.ToRadians(50f));
        // Matrix4X4<float> view = Matrix4X4<float>.Identity * Matrix4X4.CreateTranslation(0f, 0f, -3f);
        // Matrix4X4<float> projection = Matrix4X4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), 1024f / 768f, 0.1f, 100f);
        //
        // int modelLoc = _gl.GetUniformLocation(_shaderProgram.ProgramId, "model");
        // _gl.UniformMatrix4(modelLoc, 1, false, (float*)&model);
        //
        // int viewLoc = _gl.GetUniformLocation(_shaderProgram.ProgramId, "view");
        // _gl.UniformMatrix4(viewLoc, 1, false, (float*)&view);
        //
        // int projLoc = _gl.GetUniformLocation(_shaderProgram.ProgramId, "projection");
        // _gl.UniformMatrix4(projLoc, 1, false, (float*)&projection);
        //
        // _gl.DrawElements(PrimitiveType.Triangles, (uint)36, DrawElementsType.UnsignedInt, null);
    }

    public void PostRender()
    {
        _graphicsDevice.EndFrame();
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
