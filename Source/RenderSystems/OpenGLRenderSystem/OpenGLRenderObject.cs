using System.Runtime.CompilerServices;
using Duck.Graphics.Device;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLRenderObject : OpenGLRenderObjectBase
{
    #region Properties

    public override uint Id { get; }
    public override uint VertexCount => _vertexBuffer.ElementCount;
    public override uint IndexCount => _indexBuffer.ElementCount;
    public override Projection Projection { get; set; }

    #endregion

    #region Members

    private readonly OpenGLGraphicsDevice _graphicsDevice;
    private readonly IVertexBuffer _vertexBuffer;
    private readonly IIndexBuffer _indexBuffer;

    #endregion

    #region Methods

    internal unsafe OpenGLRenderObject(OpenGLGraphicsDevice graphicsDevice, IVertexBuffer vertexBuffer, IIndexBuffer indexBuffer)
    {
        _graphicsDevice = graphicsDevice;
        _vertexBuffer = vertexBuffer;
        _indexBuffer = indexBuffer;

        Id = _graphicsDevice.API.GenVertexArray();

        Bind();
        vertexBuffer.Bind();
        indexBuffer.Bind();

        var attributes = vertexBuffer.Attributes;
        var vertexSize = vertexBuffer.Stride;

        for (var i = 0; i < attributes.Length; i++) {
            _graphicsDevice.API.VertexAttribPointer(
                (uint)i,//(uint)attributes[i].Attribute,
                OpenGLUtil.ComponentCount(attributes[i].AttributeType),
                OpenGLUtil.Convert(attributes[i].AttributeType),
                attributes[i].Normalized,
                vertexSize,
                (void*)OpenGLUtil.ByteOffset(attributes, i)
            );
            OpenGLUtil.LogErrors(_graphicsDevice.API);

            _graphicsDevice.API.EnableVertexAttribArray((uint)i);//(uint)attributes[i].Attribute);
            OpenGLUtil.LogErrors(_graphicsDevice.API);
        }

        _graphicsDevice.API.BindVertexArray(0);
    }

    ~OpenGLRenderObject()
    {
        Dispose(false);
    }

    public void Bind()
    {
        ThrowIfDisposed();

        _graphicsDevice.API.BindVertexArray(Id);
        OpenGLUtil.LogErrors(_graphicsDevice.API);
    }

    public override IRenderObjectInstance CreateInstance()
    {
        return _graphicsDevice.CreateRenderObjectInstance(this);
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed) {
            throw new ObjectDisposedException("Object has been disposed");
        }
    }

    #endregion

    #region IDisposable implementation

    public override bool IsDisposed => _isDisposed;

    private bool _isDisposed = false;

    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) {
            return;
        }

        _isDisposed = true;

        _graphicsDevice.API.DeleteVertexArray(Id);
        OpenGLUtil.LogErrors(_graphicsDevice.API);

        _graphicsDevice.DestroyRenderObject(this);
    }

    #endregion
}
