using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Duck.Graphics.Components;
using Duck.Physics.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class RigidBodyLifecycleSystem_AddBox : RigidBodyLifecycleSystem
{
    #region Members

    private readonly IFilter<RigidBodyComponent, TransformComponent, BoundingBoxComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_AddBox(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<RigidBodyComponent, TransformComponent, BoundingBoxComponent>(world)
            .Build();
    }

    public override void Run()
    {
        var physics = _physicsWorld.Physics;

        foreach (var entityId in _filter.EntityAddedList) {
            IEntity entity = _filter.GetEntity(entityId);

            ref RigidBodyComponent rigidBodyComponent = ref _filter.Get1(entityId);

            TransformComponent transformComponent = _filter.Get2(entityId);
            BoundingBoxComponent boundingBoxComponent = _filter.Get3(entityId);

            CreateBody(
                entity,
                _physicsWorld,
                physics,
                PhysXHelper.CreateBoxGeometry(boundingBoxComponent, transformComponent.Scale),
                rigidBodyComponent,
                transformComponent
            );
        }
    }

    #endregion
}

public class RigidBodyLifecycleSystem_RemoveBox : RigidBodyLifecycleSystem
{
    #region Members

    private readonly IFilter<BoundingBoxComponent, PhysXIntegrationComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_RemoveBox(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<BoundingBoxComponent, PhysXIntegrationComponent>(world)
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
