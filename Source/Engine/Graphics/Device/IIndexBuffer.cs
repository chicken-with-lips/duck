using System.Diagnostics;

namespace Duck.Graphics.Device;

public interface IIndexBuffer : IBuffer
{
}

public interface IIndexBuffer<TDataType> : IIndexBuffer, IBuffer<TDataType>
    where TDataType : unmanaged
{
    
}

public class IndexBufferBuilder<TDataType>
    where TDataType : unmanaged
{
    private readonly BufferUsage _usage;

    private IndexBufferBuilder(BufferUsage usage)
    {
        _usage = usage;
    }

    public IIndexBuffer<TDataType> Build(IGraphicsDevice graphicsDevice)
    {
        return graphicsDevice.CreateIndexBuffer<TDataType>(_usage);
    }

    public static IndexBufferBuilder<TDataType> Create(BufferUsage usage)
    {
        return new IndexBufferBuilder<TDataType>(usage);
    }
}
