namespace Duck.Graphics.Device;

public interface IBuffer
{
    public void Bind();
    public Type DataType { get; }
}

public interface IBuffer<TDataType> : IBuffer
    where TDataType : unmanaged
{
    public void SetData(uint index, BufferObject<TDataType> buffer);
    public void SetData(uint index, in ReadOnlySpan<TDataType> data);
    public void SetData(uint index, in ReadOnlyMemory<TDataType> data);
}
