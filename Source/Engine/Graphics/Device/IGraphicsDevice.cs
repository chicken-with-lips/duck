using Silk.NET.Maths;

namespace Duck.Graphics.Device;

public interface IGraphicsDevice
{
    // FIXME: move out of here
    public Matrix4X4<float> ViewMatrix { get; set; }

    public void BeginFrame();
    public void Render();
    public void EndFrame();

    public IIndexBuffer<T> CreateIndexBuffer<T>(BufferUsage usage)
        where T : unmanaged;

    public IVertexBuffer<T> CreateVertexBuffer<T>(BufferUsage usage, AttributeDecl[] attributes)
        where T : unmanaged;

    public IRenderObject CreateRenderObject<TVertexType, TIndexType>(IVertexBuffer<TVertexType> vertexBuffer, IIndexBuffer<TIndexType> indexBuffer)
        where TVertexType : unmanaged
        where TIndexType : unmanaged;


    public IRenderObjectInstance CreateRenderObjectInstance(IRenderObject renderObject);

    public void ScheduleRenderable(IRenderObjectInstance renderObject);
    public void ScheduleRenderable(uint renderObjectId);
    public IRenderObjectInstance GetRenderObjectInstance(uint instanceComponentId);
}

public enum BufferUsage
{
    Static = 0
}
