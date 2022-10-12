using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics;
using Duck.Graphics.Components;
using Silk.NET.Maths;

namespace Duck.Graphics.Systems;

public class ScheduleRenderableSystem : SystemBase
{
    private readonly IFilter<RenderableInstanceComponent, TransformComponent> _filter;
    private readonly IGraphicsModule _graphicsModule;

    public ScheduleRenderableSystem(IWorld world, IGraphicsModule graphicsModule)
    {
        _graphicsModule = graphicsModule;

        _filter = Filter<RenderableInstanceComponent, TransformComponent>(world)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityList) {
            var instanceComponent = _filter.Get1(entityId);
            var transformComponent = _filter.Get2(entityId);
            var entity = _filter.GetEntity(entityId);

            var renderObjectInstance = _graphicsModule.GraphicsDevice.GetRenderObjectInstance(instanceComponent.Id);
            renderObjectInstance
                .SetParameter("WorldScale", transformComponent.Scale)
                .SetParameter("WorldPosition",
                    Matrix4X4.CreateScale(transformComponent.Scale)
                    * Matrix4X4.CreateFromQuaternion(transformComponent.Rotation)
                    * Matrix4X4.CreateTranslation(transformComponent.Position)
                );

            if (entity.Has<BoundingBoxComponent>()) {
                var boundingBox = entity.Get<BoundingBoxComponent>();

                var scaledBoundingBox = new BoundingBoxComponent() {
                    Box = boundingBox.Box.GetScaled(transformComponent.Scale, boundingBox.Box.Center)
                };

                renderObjectInstance.BoundingVolume = boundingBox;
            } else if (entity.Has<BoundingSphereComponent>()) {
                var boundingSphere = entity.Get<BoundingSphereComponent>();

                // var scaledBoundingSphere = new BoundingSphereComponent() {
                //     Radius = boundingSphere.Radius * MathF.Max(MathF.Max(transformComponent.Scale.X, transformComponent.Scale.Y), transformComponent.Scale.Z)
                // };
                var scaledBoundingSphere = new BoundingSphereComponent() {
                    Radius = boundingSphere.Radius
                };

                renderObjectInstance.BoundingVolume = boundingSphere;
            }
        }
    }
}
