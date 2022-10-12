using Duck.Graphics.Device;
using Silk.NET.OpenGL;

namespace Duck.Graphics.OpenGL;

public class OpenGLRenderObject : OpenGLRenderObjectBase
{
    #region Properties

    public override uint Id { get; }
    public override uint VertexCount { get; }
    public override uint IndexCount { get; }
    public override Projection Projection { get; set; }

    #endregion

    #region Members

    private readonly OpenGLGraphicsDevice _graphicsDevice;

    #endregion

    #region Methods

    internal unsafe OpenGLRenderObject(OpenGLGraphicsDevice graphicsDevice, IVertexBuffer vertexBuffer, IIndexBuffer indexBuffer)
    {
        _graphicsDevice = graphicsDevice;

        Id = _graphicsDevice.API.GenVertexArray();
        VertexCount = vertexBuffer.ElementCount;
        IndexCount = indexBuffer.ElementCount;

        Bind();
        vertexBuffer.Bind();
        indexBuffer.Bind();

        var attributes = vertexBuffer.Attributes;
        var vertexSize = OpenGLUtil.VertexSize(attributes);

        for (var i = 0; i < attributes.Length; i++) {
            _graphicsDevice.API.VertexAttribPointer(
                (uint)attributes[i].Attribute,
                OpenGLUtil.ComponentCount(attributes[i].AttributeType),
                OpenGLUtil.Convert(attributes[i].AttributeType),
                attributes[i].Normalized,
                vertexSize,
                (void*)OpenGLUtil.ByteOffset(attributes, i)
            );
            OpenGLUtil.LogErrors(_graphicsDevice.API);

            _graphicsDevice.API.EnableVertexAttribArray((uint)attributes[i].Attribute);
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
