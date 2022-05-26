using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class RigidBodySynchronizationSystem : SystemBase
{
    #region Members

    private readonly IFilter<PhysXIntegrationComponent, TransformComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodySynchronizationSystem(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<PhysXIntegrationComponent, TransformComponent>(world)
            .Build();
    }

    public override void Run()
    {
        // var simulation = _physicsWorld.Simulation;

        foreach (var entityId in _filter.EntityList) {
            var physxComponent = _filter.Get1(entityId);
            ref TransformComponent transform = ref _filter.Get2(entityId);
            var pxTransform = physxComponent.Body.GlobalPose;

            if (transform.IsPositionDirty || transform.IsRotationDirty) {
                physxComponent.Body.GlobalPose = new PxTransform(
                    transform.IsRotationDirty ? transform.Rotation.ToSystem() : pxTransform.Quaternion,
                    transform.IsPositionDirty ? transform.Position.ToSystem() : pxTransform.Position
                );
            } else {
                transform.Position = pxTransform.Position.ToGeneric();
                transform.Rotation = pxTransform.Quaternion.ToGeneric();
            }

            transform.ClearDirtyFlags();
        }
    }

    #endregion
}
