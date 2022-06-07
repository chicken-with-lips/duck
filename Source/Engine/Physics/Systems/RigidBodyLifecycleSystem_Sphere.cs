using System.Numerics;
using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class RigidBodyLifecycleSystem_Sphere : RigidBodyLifecycleSystem
{
    #region Members

    private readonly IFilter<RigidBodyComponent, TransformComponent, BoundingSphereComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_Sphere(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<RigidBodyComponent, TransformComponent, BoundingSphereComponent>(world)
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
            BoundingSphereComponent sphereComponent = _filter.Get3(entityId);

            var geometry = CreateGeometry(
                sphereComponent,
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

    private static PxGeometry CreateGeometry(BoundingSphereComponent sphere, Vector3D<float> scale)
    {
        return new PxSphereGeometry(sphere.Radius);
    }

    #endregion
}
