using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Graphics.Components;
using Silk.NET.Maths;

namespace Duck.Graphics.Systems;

public partial class ScheduleRenderableSystem : BaseSystem<World, float>
{
    private readonly IGraphicsModule _graphicsModule;

    public ScheduleRenderableSystem(World world, IGraphicsModule graphicsModule)
        : base(world)
    {
        _graphicsModule = graphicsModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity /*, in TransformComponent transform, in RenderableInstanceComponent instance*/)
    {
        /*var instanceComponent = _filter.Get1(entityId);

            var renderObjectInstance = _graphicsModule.GraphicsDevice.GetRenderObjectInstance(instanceComponent.Id);
            renderObjectInstance
                .SetParameter("WorldScale", transform.Scale)
                .SetParameter("WorldPosition",
                    Matrix4X4.CreateScale(transform.Scale)
                    * Matrix4X4.CreateFromQuaternion(transform.Rotation)
                    * Matrix4X4.CreateTranslation(transform.Position)
                );

            if (entity.Has<BoundingBoxComponent>()) {
                var boundingBox = entity.Get<BoundingBoxComponent>();

                var scaledBoundingBox = new BoundingBoxComponent() {
                    Box = boundingBox.Box.GetScaled(transform.Scale, boundingBox.Box.Center)
                };

                renderObjectInstance.BoundingVolume = boundingBox;
            } else if (entity.Has<BoundingSphereComponent>()) {
                var boundingSphere = entity.Get<BoundingSphereComponent>();

                // var scaledBoundingSphere = new BoundingSphereComponent() {
                //     Radius = boundingSphere.Radius * MathF.Max(MathF.Max(transform.Scale.X, transform.Scale.Y), transform.Scale.Z)
                // };
                var scaledBoundingSphere = new BoundingSphereComponent() {
                    Radius = boundingSphere.Radius
                };

                renderObjectInstance.BoundingVolume = boundingSphere;
            }

            Console.WriteLine("SC");
            _graphicsModule.GraphicsDevice.ScheduleRenderableInstance(instanceComponent.Id);*/
    }
}
