using Duck.Graphics.Device;
using Silk.NET.Maths;

namespace Duck.RenderSystems.Wicked;

public class WickedGraphicsDevice : IGraphicsDevice
{
    public Matrix4X4<float> ViewMatrix {
        get;
        set;
    }

    public IIndexBuffer<T> CreateIndexBuffer<T>(BufferUsage usage) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IVertexBuffer<T> CreateVertexBuffer<T>(BufferUsage usage, AttributeDecl[] attributes) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IRenderObject CreateRenderObject<TVertexType, TIndexType>(IVertexBuffer<TVertexType> vertexBuffer, IIndexBuffer<TIndexType> indexBuffer) where TVertexType : unmanaged where TIndexType : unmanaged
    {
        throw new NotImplementedException();
    }

    public void DestroyRenderObject(IRenderObject renderObject)
    {
        throw new NotImplementedException();
    }

    public IRenderObjectInstance CreateRenderObjectInstance(IRenderObject renderObject)
    {
        throw new NotImplementedException();
    }

    public void ScheduleRenderable(IRenderObject renderObject)
    {
        throw new NotImplementedException();
    }

    public void ScheduleRenderableInstance(uint renderObjectInstanceId)
    {
        throw new NotImplementedException();
    }

    public IRenderObjectInstance GetRenderObjectInstance(uint instanceComponentId)
    {
        throw new NotImplementedException();
    }
}
