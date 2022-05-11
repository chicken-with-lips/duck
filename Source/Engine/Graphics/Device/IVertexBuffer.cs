using System.Diagnostics;

namespace Duck.Graphics.Device;

public interface IVertexBuffer : IBuffer
{
    public AttributeDecl[] Attributes { get; }
}

public interface IVertexBuffer<TDataType> : IVertexBuffer, IBuffer<TDataType>
    where TDataType : unmanaged
{
}

public class VertexBufferBuilder<TDataType>
    where TDataType : unmanaged
{
    private readonly BufferUsage _usage;
    private List<AttributeDecl> _attributes = new();

    private VertexBufferBuilder(BufferUsage usage)
    {
        _usage = usage;
    }

    public VertexBufferBuilder<TDataType> Attribute(VertexAttribute attribute, uint bufferIndex, AttributeType attributeType)
    {
        Debug.Assert(bufferIndex == 0, "bufferIndex must be 0");

        _attributes.Add(
            new AttributeDecl(
                attribute,
                bufferIndex,
                attributeType
            )
        );

        return this;
    }

    public IVertexBuffer<TDataType> Build(IGraphicsDevice graphicsDevice)
    {
        Debug.Assert(_attributes.Count > 0);

        return graphicsDevice.CreateVertexBuffer<TDataType>(_usage, _attributes.ToArray());
    }

    public static VertexBufferBuilder<TDataType> Create(BufferUsage usage)
    {
        return new VertexBufferBuilder<TDataType>(usage);
    }
}

public enum VertexAttribute : byte
{
    Position = 0,
    Uv0 = 1,
}

public enum AttributeType
{
    Float2,
    Float3,
}

public readonly struct AttributeDecl
{
    public VertexAttribute Attribute { get; }
    public uint BufferIndex { get; }
    public AttributeType AttributeType { get; }

    public AttributeDecl(VertexAttribute attribute, uint bufferIndex, AttributeType attributeType)
    {
        Attribute = attribute;
        BufferIndex = bufferIndex;
        AttributeType = attributeType;
    }
}
