using System.Diagnostics;
using Duck.Graphics.Device;
using Silk.NET.OpenGL;

namespace Duck.Graphics.OpenGL;

public abstract class OpenGLBufferBase<TDataType> : IBuffer<TDataType>, IDisposable
    where TDataType : unmanaged
{
    #region Properties

    public Type DataType => typeof(TDataType);
    public uint ElementCount { get; private set; }

    #endregion

    #region Members

    private readonly OpenGLGraphicsDevice _graphicsDevice;
    private readonly GL _api;
    private readonly BufferUsageARB _usage;
    private readonly BufferTargetARB _bufferType;

    private readonly uint _id;

    #endregion

    #region Methods

    internal OpenGLBufferBase(OpenGLGraphicsDevice graphicsDevice, GL api, BufferUsageARB usage, BufferTargetARB bufferType)
    {
        _graphicsDevice = graphicsDevice;
        _usage = usage;
        _bufferType = bufferType;
        _api = api;

        _id = _api.GenBuffer();
    }

    ~OpenGLBufferBase()
    {
        Dispose(false);
    }

    public void Bind()
    {
        _api.BindBuffer(_bufferType, _id);
    }

    public void SetData(uint index, BufferObject<TDataType> buffer)
    {
        SetData(index, buffer.Data);
    }

    public unsafe void SetData(uint index, in ReadOnlySpan<TDataType> data)
    {
        Debug.Assert(index == 0, "Index must be zero");

        Bind();

        _api.BufferData(_bufferType, (nuint)(data.Length * sizeof(TDataType)), data, _usage);
        ElementCount = (uint)data.Length;
    }

    public unsafe void SetData(uint index, in ReadOnlyMemory<TDataType> data)
    {
        Debug.Assert(index == 0, "Index must be zero");

        Bind();

        _api.BufferData<TDataType>(_bufferType, (nuint)(data.Length * sizeof(TDataType)), data.ToArray(), _usage);
        ElementCount = (uint)data.Length;
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

        _api.DeleteBuffer(_id);

        _isDisposed = true;
    }

    #endregion
}
