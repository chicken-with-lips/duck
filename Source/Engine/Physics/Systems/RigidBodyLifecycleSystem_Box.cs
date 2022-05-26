using System.Numerics;
using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class RigidBodyLifecycleSystem_Box : RigidBodyLifecycleSystem
{
    #region Members

    private readonly IFilter<RigidBodyComponent, TransformComponent, BoundingBoxComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_Box(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<RigidBodyComponent, TransformComponent, BoundingBoxComponent>(world)
            .Build();
    }

    public override void Run()
    {
        var physics = _physicsWorld.Physics;
        var scene = _physicsWorld.Scene;

        foreach (var entityId in _filter.EntityAddedList) {
            IEntity entity = _filter.GetEntity(entityId);

            ref RigidBodyComponent rigidBodyComponent = ref _filter.Get1(entityId);
            ref PhysXIntegrationComponent physxComponent = ref entity.Get<PhysXIntegrationComponent>();

            TransformComponent transformComponent = _filter.Get2(entityId);
            BoundingBoxComponent boundingBoxComponent = _filter.Get3(entityId);

            var geometry = CreateGeometry(
                boundingBoxComponent.Box,
                transformComponent.Scale
            );

            var body = CreateBody(
                entity,
                _physicsWorld,
                physics,
                ref physxComponent,
                geometry,
                rigidBodyComponent,
                transformComponent
            );

            scene.AddActor(body);
        }
    }

    private static PxBoxGeometry CreateGeometry(Box3D<float> boundingBox, Vector3D<float> scale)
    {
        var x = (MathF.Abs(boundingBox.Min.X) + MathF.Abs(boundingBox.Max.X)) * scale.X;
        var y = (MathF.Abs(boundingBox.Min.Y) + MathF.Abs(boundingBox.Max.Y)) * scale.Y;
        var z = (MathF.Abs(boundingBox.Min.Z) + MathF.Abs(boundingBox.Max.Z)) * scale.Z;

        return new PxBoxGeometry(new Vector3(x / 2f, y / 2f, z / 2f));
    }

    #endregion
}
