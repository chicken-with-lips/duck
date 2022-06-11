using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics.Components;
using Duck.Physics.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class RigidBodySynchronizationSystem : SystemBase
{
    #region Members

    private readonly IFilter<RigidBodyComponent, PhysXIntegrationComponent, TransformComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodySynchronizationSystem(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<RigidBodyComponent, PhysXIntegrationComponent, TransformComponent>(world)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityList) {
            ref var rigidBodyComponent = ref _filter.Get1(entityId);
            var physxComponent = _filter.Get2(entityId);
            ref TransformComponent transform = ref _filter.Get3(entityId);
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

            if (rigidBodyComponent.Type == RigidBodyComponent.BodyType.Dynamic && physxComponent.Body is PxRigidDynamic dynamic) {
                dynamic.AddForce(rigidBodyComponent.AccumulatedAccelerationForce.ToSystem(), PxForceMode.Acceleration);
                dynamic.AddForce(rigidBodyComponent.AccumulatedForceForce.ToSystem(), PxForceMode.Force);
                dynamic.AddForce(rigidBodyComponent.AccumulatedImpulseForce.ToSystem(), PxForceMode.Impulse);
                dynamic.AddForce(rigidBodyComponent.AccumulatedVelocityChangeForce.ToSystem(), PxForceMode.VelocityChange);

                dynamic.AddTorque(rigidBodyComponent.AccumulatedAccelerationTorque.ToSystem(), PxForceMode.Acceleration);
                dynamic.AddTorque(rigidBodyComponent.AccumulatedForceTorque.ToSystem(), PxForceMode.Force);
                dynamic.AddTorque(rigidBodyComponent.AccumulatedImpulseTorque.ToSystem(), PxForceMode.Impulse);
                dynamic.AddTorque(rigidBodyComponent.AccumulatedVelocityChangeTorque.ToSystem(), PxForceMode.VelocityChange);
            }

            rigidBodyComponent.ClearAccumulatedForces();

            physxComponent.Body.SetActorFlag(PxActorFlag.DisableGravity, !rigidBodyComponent.IsGravityEnabled);
        }
    }

    #endregion
}
