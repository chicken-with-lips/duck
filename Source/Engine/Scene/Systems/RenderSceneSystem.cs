using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Content;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Scene.Components;

namespace Duck.Scene.Systems;

public partial class RenderSceneSystem : BaseSystem<World, float>
{
    private readonly IGraphicsDevice _graphicsDevice;
    private readonly IContentModule _contentModule;

    public RenderSceneSystem(World world, IGraphicsDevice graphicsDevice)
        : base(world)
    {
        _graphicsDevice = graphicsDevice;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderStaticMeshWithBoundingBox(in TransformComponent transform, in RuntimeStaticMeshComponent runtimeStaticMesh, in BoundingBoxComponent boundingBox)
    {
        var renderObjectInstance = _graphicsDevice.GetRenderObjectInstance(runtimeStaticMesh.InstanceId);
        renderObjectInstance
            .SetParameter("WorldScale", transform.Scale)
            .SetParameter("WorldPosition", transform.WorldTranslation);

        /*var scaledBoundingBox = new BoundingBoxComponent() {
            Box = boundingBox.Box.GetScaled(transform.Scale, boundingBox.Box.Center)
        };*/

        renderObjectInstance.BoundingVolume = boundingBox;

        _graphicsDevice.ScheduleRenderableInstance(runtimeStaticMesh.InstanceId);
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderStaticMeshWithBoundingSphere(in TransformComponent transform, in RuntimeStaticMeshComponent runtimeStaticMesh, in BoundingSphereComponent boundingSphere)
    {
        var renderObjectInstance = _graphicsDevice.GetRenderObjectInstance(runtimeStaticMesh.InstanceId);
        renderObjectInstance
            .SetParameter("WorldScale", transform.Scale)
            .SetParameter("WorldPosition", transform.WorldTranslation);

        /*var scaledBoundingSphere = new BoundingSphereComponent() {
            Radius = boundingSphere.Radius
        };*/

        renderObjectInstance.BoundingVolume = boundingSphere;

        _graphicsDevice.ScheduleRenderableInstance(runtimeStaticMesh.InstanceId);
    }
}
