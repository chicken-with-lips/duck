using System.Diagnostics;
using Silk.NET.Maths;

namespace Duck.Graphics.Device;

public interface IVertexBuffer : IBuffer, IDisposable
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
    Normal = 1,
    Uv0 = 2,
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

public readonly struct TexturedVertex
{
    public readonly Vector3D<float> Position;
    public readonly Vector3D<float> Normal;
    public readonly Vector2D<float> TexCoords;

    public TexturedVertex(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoords)
    {
        Position = position;
        Normal = normal;
        TexCoords = texCoords;
    }
}

public readonly struct DebugVertex
{
    public readonly Vector3D<float> Position;

    public DebugVertex(Vector3D<float> position)
    {
        Position = position;
    }
}
