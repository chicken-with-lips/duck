using System.Diagnostics;
using System.Runtime.CompilerServices;
using Duck.Renderer.Device;
using Silk.NET.OpenGL;

namespace Duck.RenderSystems.OpenGL;

internal abstract class OpenGLBufferBase<TDataType> : IBuffer<TDataType>, IDisposable
    where TDataType : unmanaged
{
    #region Properties

    public Type DataType => typeof(TDataType);
    public uint ElementCount { get; private set; }
    public uint Stride => (uint) Unsafe.SizeOf<TDataType>();

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
        OpenGLUtil.LogErrors(_api);
    }

    public void SetData(uint index, BufferObject<TDataType> buffer)
    {
        SetData(index, buffer.Data);
    }

    public void SetData(uint index, in TDataType[] buffer)
    {
        SetData(index, buffer.AsSpan());
    }

    public unsafe void SetData(uint index, in ReadOnlySpan<TDataType> data)
    {
        Debug.Assert(index == 0, "Index must be zero");

        Bind();

        _api.BufferData(_bufferType, (nuint)(data.Length * sizeof(TDataType)), data, _usage);
        OpenGLUtil.LogErrors(_api);

        ElementCount = (uint)data.Length;
    }

    public unsafe void SetData(uint index, in ReadOnlyMemory<TDataType> data)
    {
        Debug.Assert(index == 0, "Index must be zero");

        Bind();

        _api.BufferData<TDataType>(_bufferType, (nuint)(data.Length * sizeof(TDataType)), data.ToArray(), _usage);
        OpenGLUtil.LogErrors(_api);

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

        // _api.DeleteBuffer(_id);
        // OpenGLUtil.LogErrors(_api);

        _isDisposed = true;
    }

    #endregion
}
