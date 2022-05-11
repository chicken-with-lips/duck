using System.Runtime.InteropServices;
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
            _ => throw new NotImplementedException()
        };
    }

    public static int ComponentCount(AttributeType type)
    {
        return type switch {
            AttributeType.Float2 => 2,
            AttributeType.Float3 => 3,
            _ => throw new NotImplementedException()
        };
    }

    public static unsafe uint VertexSize(Type type, AttributeDecl[] attributes)
    {
        uint size = 0;
        int vertexSize = Marshal.SizeOf(type);

        for (var i = 0; i < attributes.Length; i++) {
            size += (uint)(ComponentCount(attributes[i].AttributeType) * vertexSize);
        }

        return size;
    }

    public static unsafe uint VertexSize<TVertexType>(AttributeDecl[] attributes)
        where TVertexType : unmanaged
    {
        uint size = 0;
        int vertexSize = sizeof(TVertexType);

        for (var i = 0; i < attributes.Length; i++) {
            size += (uint)(ComponentCount(attributes[i].AttributeType) * vertexSize);
        }

        return size;
    }

    public static int ByteOffset(Type type, in AttributeDecl[] attributes, int attributeIndex)
    {
        var offset = 0;
        var vertexSize = Marshal.SizeOf(type);

        for (var i = 0; i < attributeIndex; i++) {
            offset += ComponentCount(attributes[i].AttributeType) * vertexSize;
        }

        return offset;
    }

    public static unsafe int ByteOffset<TVertexType>(in AttributeDecl[] attributes, int attributeIndex)
        where TVertexType : unmanaged
    {
        var offset = 0;
        var vertexSize = sizeof(TVertexType);

        for (var i = 0; i < attributeIndex; i++) {
            offset += ComponentCount(attributes[i].AttributeType) * vertexSize;
        }

        return offset;
    }
}
