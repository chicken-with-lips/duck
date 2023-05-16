namespace Duck.Renderer.Device;

public class BufferObject<TDataType>
    where TDataType : unmanaged
{
    #region Properties

    public ReadOnlyMemory<TDataType> Data { get; }

    #endregion

    #region Methods

    public BufferObject(ReadOnlyMemory<TDataType> data)
    {
        Data = data;
    }

    #endregion
}
