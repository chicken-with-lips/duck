namespace Duck.Graphics.Device;

public interface IBuffer
{
    public Type DataType { get; }
    public uint ElementCount { get; }

    public void Bind();
}

public interface IBuffer<TDataType> : IBuffer
    where TDataType : unmanaged
{
    public void SetData(uint index, BufferObject<TDataType> buffer);
    public void SetData(uint index, in ReadOnlySpan<TDataType> data);
    public void SetData(uint index, in ReadOnlyMemory<TDataType> data);
}
