using Duck.Graphics.Device;
using Silk.NET.OpenGL;
using AttributeType = Duck.Graphics.Device.AttributeType;

namespace Duck.Graphics.OpenGL;

public static class OpenGLUtil
{
    public static BufferUsageARB Convert(BufferUsage usage)
    {
        return usage switch {
            BufferUsage.Static => BufferUsageARB.StaticRead,
            _ => throw new NotImplementedException()
        };
    }

    public static VertexAttribPointerType Convert(AttributeType type)
    {
        return type switch {
            AttributeType.Float2 => VertexAttribPointerType.Float,
            AttributeType.Float3 => VertexAttribPointerType.Float,
            AttributeType.Float4 => VertexAttribPointerType.Float,
            AttributeType.UnsignedByte4 => VertexAttribPointerType.UnsignedByte,
            _ => throw new NotImplementedException()
        };
    }

    public static int ComponentCount(AttributeType type)
    {
        return type switch {
            AttributeType.Float2 => 2,
            AttributeType.Float3 => 3,
            AttributeType.Float4 => 4,
            AttributeType.UnsignedByte4 => 4,
            _ => throw new NotImplementedException()
        };
    }

    public static int ComponentSize(AttributeType type)
    {
        return type switch {
            AttributeType.Float2 => 4,
            AttributeType.Float3 => 4,
            AttributeType.Float4 => 4,
            AttributeType.UnsignedByte4 => 1,
            _ => throw new NotImplementedException()
        };
    }

    public static uint VertexSize(AttributeDecl[] attributes)
    {
        uint size = 0;

        for (var i = 0; i < attributes.Length; i++) {
            size += (uint)(ComponentCount(attributes[i].AttributeType) * ComponentSize(attributes[i].AttributeType));
        }

        return size;
    }

    public static int ByteOffset(in AttributeDecl[] attributes, int attributeIndex)
    {
        var offset = 0;

        for (var i = 0; i < attributeIndex; i++) {
            offset += ComponentCount(attributes[i].AttributeType) * ComponentSize(attributes[i].AttributeType);
        }

        return offset;
    }
}
