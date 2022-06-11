using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Duck.Graphics.Components;
using Duck.Physics.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class RigidBodyLifecycleSystem_AddSphere : RigidBodyLifecycleSystem
{
    #region Members

    private readonly IFilter<RigidBodyComponent, TransformComponent, BoundingSphereComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_AddSphere(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<RigidBodyComponent, TransformComponent, BoundingSphereComponent>(world)
            .Build();
    }

    public override void Run()
    {
        var physics = _physicsWorld.Physics;

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

            CreateBody(
                entity,
                _physicsWorld,
                physics,
                geometry,
                rigidBodyComponent,
                transformComponent
            );
        }
    }

    private static PxGeometry CreateGeometry(BoundingSphereComponent sphere, Vector3D<float> scale)
    {
        return new PxSphereGeometry(sphere.Radius * MathF.Max(MathF.Max(scale.X, scale.Y), scale.Z));
    }

    #endregion
}

public class RigidBodyLifecycleSystem_RemoveSphere : RigidBodyLifecycleSystem
{
    #region Members

    private readonly IFilter<BoundingSphereComponent, PhysXIntegrationComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_RemoveSphere(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<BoundingSphereComponent, PhysXIntegrationComponent>(world)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityRemovedList) {
            RemoveBody(ref _filter.Get2(entityId), _physicsWorld);
        }
    }

    #endregion
}
