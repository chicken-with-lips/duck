using System.Diagnostics;
using System.Drawing;
using Silk.NET.Maths;

namespace Duck.Renderer.Device;

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

    public VertexBufferBuilder<TDataType> Attribute(VertexAttribute attribute, uint bufferIndex, AttributeType attributeType, bool normalized = false)
    {
        Debug.Assert(bufferIndex == 0, "bufferIndex must be 0");

        _attributes.Add(
            new AttributeDecl(
                attribute,
                bufferIndex,
                attributeType,
                normalized
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
    TexCoord0 = 2,
    Color0 = 3,
}

public enum AttributeType
{
    Float2,
    Float3,
    Float4,
    UnsignedByte4,
}

public readonly struct AttributeDecl
{
    public VertexAttribute Attribute { get; }
    public uint BufferIndex { get; }
    public AttributeType AttributeType { get; }
    public bool Normalized { get; }

    public AttributeDecl(VertexAttribute attribute, uint bufferIndex, AttributeType attributeType, bool normalized)
    {
        Attribute = attribute;
        BufferIndex = bufferIndex;
        AttributeType = attributeType;
        Normalized = normalized;
    }
}

public readonly struct TexturedVertex
{
    public readonly Vector3D<float> Position;
    public readonly Vector3D<float> Normal;
    public readonly Vector2D<float> TexCoords;
    public readonly Vector4D<float> BaseColor;

    public TexturedVertex(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoords)
        : this(position, normal, texCoords, Color.White.ToVector())
    {
    }

    public TexturedVertex(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoords, Vector4D<float> color)
    {
        Position = position;
        Normal = normal;
        TexCoords = texCoords;
        BaseColor = color;
    }
}

public readonly struct ColoredVertex
{
    public readonly Vector3D<float> Position;
    public readonly Vector4D<float> Color;

    public ColoredVertex(Vector3D<float> position, Vector4D<float> color)
    {
        Position = position;
        Color = color;
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
