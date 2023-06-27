using Duck.Renderer.Components;
using Silk.NET.Maths;

namespace Duck.Renderer.Device;

public interface IGraphicsDevice
{
    public IIndexBuffer<T> CreateIndexBuffer<T>(BufferUsage usage)
        where T : unmanaged;

    public IVertexBuffer<T> CreateVertexBuffer<T>(BufferUsage usage, AttributeDecl[] attributes)
        where T : unmanaged;

    public IRenderObject CreateRenderObject<TVertexType, TIndexType>(IVertexBuffer<TVertexType> vertexBuffer, IIndexBuffer<TIndexType> indexBuffer)
        where TVertexType : unmanaged
        where TIndexType : unmanaged;

    public IRenderObjectInstance GetRenderObjectInstance(uint instanceComponentId);

    public CommandBuffer CreateCommandBuffer(View view);

    public void Render(CommandBuffer commandBuffer);
}

public enum BufferUsage
{
    Static,
    Dynamic
}
