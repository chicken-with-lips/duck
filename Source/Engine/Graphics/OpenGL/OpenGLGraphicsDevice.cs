using System.Diagnostics;
using Duck.Graphics.Device;
using Duck.Graphics.Textures;
using Duck.Math;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Duck.Graphics.OpenGL;

public class OpenGLGraphicsDevice : IGraphicsDevice
{
    #region Properties

    internal GL API => _api;

    public Matrix4X4<float> ViewMatrix { get; set; }

    #endregion

    #region Members

    private readonly GL _api;
    private readonly IGLContext _context;

    private readonly Dictionary<uint, OpenGLRenderObject> _renderObjects = new();
    private readonly Dictionary<uint, OpenGLRenderObjectInstance> _renderObjectInstances = new();
    private readonly List<OpenGLRenderObjectInstance> _frameRenderables = new();

    private uint _renderObjectInstanceCounter = 0;

    #endregion

    #region Methods

    public OpenGLGraphicsDevice(IGLContext context)
    {
        _context = context;
        _api = GL.GetApi(_context);
    }

    public void BeginFrame()
    {
        _frameRenderables.Clear();

        _context.MakeCurrent();

        _api.Enable(GLEnum.DepthTest);
        _api.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public unsafe void Render()
    {
        // TODO: move out to render graph

        foreach (var renderObjectInstance in _frameRenderables) {
            var renderObject = _renderObjects[renderObjectInstance.ParentId];
            renderObject.Bind();

            if (renderObjectInstance.GetShaderProgram() is OpenGLShaderProgram glShaderProgram) {
                _api.UseProgram(glShaderProgram.ProgramId);

                // Matrix4X4<float> trans = Matrix4X4<float>.Identity;
                // trans *= Matrix4X4.CreateFromAxisAngle(Vector3D<float>.UnitZ, MathHelper.ToRadians(Time.Elapsed));
                // trans *= Matrix4X4.CreateScale(0.5f, 0.5f, 0.5f);
                // trans *= Matrix4X4.CreateTranslation(0.5f, -0.5f, 0f);
                //
                //
                // Matrix4X4<float> model = Matrix4X4<float>.Identity * Matrix4X4.CreateRotationX(Time.Elapsed * MathHelper.ToRadians(50f));
                Matrix4X4<float> model = renderObjectInstance.HasParameter("WorldPosition") ? renderObjectInstance.GetParameterMatrix4X4("WorldPosition") : Matrix4X4<float>.Identity;
                // Matrix4X4<float> view = Matrix4X4<float>.Identity * Matrix4X4.CreateTranslation(0f, -3, -3f);
                Matrix4X4<float> view = this.ViewMatrix;
                
                Matrix4X4<float> projection = Matrix4X4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(75f), 1024f / 768f, 0.1f, 10000f);
                //
                int modelLoc = _api.GetUniformLocation(glShaderProgram.ProgramId, "in_Model");
                _api.UniformMatrix4(modelLoc, 1, false, (float*)&model);
                //
                int viewLoc = _api.GetUniformLocation(glShaderProgram.ProgramId, "in_View");
                _api.UniformMatrix4(viewLoc, 1, false, (float*)&view);
                //
                int projLoc = _api.GetUniformLocation(glShaderProgram.ProgramId, "in_Projection");
                _api.UniformMatrix4(projLoc, 1, false, (float*)&projection);
            }

            if (renderObjectInstance.GetTexture(0) is OpenGLTexture2D glTexture2D) {
                _api.BindTexture(TextureTarget.Texture2D, glTexture2D.TextureId);
            }

            _api.DrawElements(PrimitiveType.Triangles, renderObject.IndexCount, DrawElementsType.UnsignedInt, null);
        }
    }

    public void EndFrame()
    {
        _context.SwapBuffers();
    }

    public IIndexBuffer<T> CreateIndexBuffer<T>(BufferUsage usage)
        where T : unmanaged
    {
        return new OpenGLIndexBuffer<T>(this, _api, OpenGLUtil.Convert(usage));
    }

    public IVertexBuffer<T> CreateVertexBuffer<T>(BufferUsage usage, AttributeDecl[] attributes)
        where T : unmanaged
    {
        return new OpenGLVertexBuffer<T>(this, _api, OpenGLUtil.Convert(usage), attributes);
    }

    public IRenderObject CreateRenderObject<TDataType, TIndexType>(IVertexBuffer<TDataType> vertexBuffer, IIndexBuffer<TIndexType> indexBuffer)
        where TDataType : unmanaged
        where TIndexType : unmanaged
    {
        var obj = new OpenGLRenderObject(this, vertexBuffer, indexBuffer);
        _renderObjects.Add(obj.Id, obj);

        return obj;
    }

    public IRenderObjectInstance CreateRenderObjectInstance(IRenderObject renderObject)
    {
        var instance = new OpenGLRenderObjectInstance(++_renderObjectInstanceCounter, renderObject);
        _renderObjectInstances.Add(instance.Id, instance);

        return instance;
    }

    public void ScheduleRenderable(IRenderObjectInstance instance)
    {
        _frameRenderables.Add(instance as OpenGLRenderObjectInstance);
    }

    public void ScheduleRenderable(uint instanceId)
    {
        _frameRenderables.Add(
            _renderObjectInstances[instanceId]
        );
    }

    public IRenderObjectInstance GetRenderObjectInstance(uint instanceComponentId)
    {
        return _renderObjectInstances[instanceComponentId];
    }

    #endregion
}
