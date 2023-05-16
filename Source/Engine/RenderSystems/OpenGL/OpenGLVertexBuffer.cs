using Duck.Renderer.Device;
using Silk.NET.OpenGL;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLVertexBuffer<TDataType> : OpenGLBufferBase<TDataType>, IVertexBuffer<TDataType>
    where TDataType : unmanaged
{
    public AttributeDecl[] Attributes { get; }

    internal OpenGLVertexBuffer(OpenGLGraphicsDevice graphicsDevice, GL api, BufferUsageARB usage, AttributeDecl[] attributes)
        : base(graphicsDevice, api, usage, BufferTargetARB.ArrayBuffer)
    {
        Attributes = attributes;
    }
}
