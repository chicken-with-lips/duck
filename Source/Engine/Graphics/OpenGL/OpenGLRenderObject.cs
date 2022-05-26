using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Shaders;

namespace Duck.Graphics.OpenGL;

public class OpenGLRenderObject : OpenGLRenderObjectBase, IDisposable
{
    #region Properties

    public override uint Id { get; }
    public override uint VertexCount { get; }
    public override uint IndexCount { get; }

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

        // sort by index
        Array.Sort(attributes, (a, b) => a.Attribute.CompareTo(b.Attribute));

        var vertexSize = OpenGLUtil.VertexSize(attributes);

        for (var i = 0; i < attributes.Length; i++) {
            _graphicsDevice.API.VertexAttribPointer(
                (uint)attributes[i].Attribute,
                OpenGLUtil.ComponentCount(attributes[i].AttributeType),
                OpenGLUtil.Convert(attributes[i].AttributeType),
                false,
                vertexSize,
                (void*)OpenGLUtil.ByteOffset(attributes, i)
            );

            _graphicsDevice.API.EnableVertexAttribArray((uint)attributes[i].Attribute);
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
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed) {
            throw new ObjectDisposedException("Object has been disposed");
        }
    }

    #endregion

    #region IDisposable implementation

    private bool _isDisposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) {
            return;
        }

        _graphicsDevice.API.DeleteVertexArray(Id);

        _isDisposed = true;
    }

    #endregion
}
