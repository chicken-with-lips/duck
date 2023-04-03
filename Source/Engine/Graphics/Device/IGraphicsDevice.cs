using Duck.Graphics.Components;
using Silk.NET.Maths;

namespace Duck.Graphics.Device;

public interface IGraphicsDevice
{
    public Matrix4X4<float> ViewMatrix { get; set; }

    public IIndexBuffer<T> CreateIndexBuffer<T>(BufferUsage usage)
        where T : unmanaged;

    public IVertexBuffer<T> CreateVertexBuffer<T>(BufferUsage usage, AttributeDecl[] attributes)
        where T : unmanaged;

    public IRenderObject CreateRenderObject<TVertexType, TIndexType>(IVertexBuffer<TVertexType> vertexBuffer, IIndexBuffer<TIndexType> indexBuffer)
        where TVertexType : unmanaged
        where TIndexType : unmanaged;

    public void ScheduleRenderable(IRenderObject renderObject);
    public void ScheduleRenderableInstance(uint renderObjectInstanceId);
    public IRenderObjectInstance GetRenderObjectInstance(uint instanceComponentId);
}

public enum BufferUsage
{
    Static = 0
}
