using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Physics.Components;
using Duck.Graphics.Components;
using Silk.NET.Maths;
using MathF = Duck.Math.MathF;

namespace Duck.Physics.Systems;

public partial class PhysXPushChangesSystem : BaseSystem<World, float>
{
    private readonly PhysicsWorld _physicsWorld;

    #region Methods

    public PhysXPushChangesSystem(World world, PhysicsWorld physicsWorld)
        : base(world)
    {
        _physicsWorld = physicsWorld;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(ref RigidBodyComponent rigidBody, in PhysXIntegrationComponent physXIntegration, ref TransformComponent transform)
    {
        /*var pxBody = _physicsWorld.GetActor(physXIntegration.BodyId);
        var pxTransform = pxBody.GlobalPose;

        if (transform.IsPositionDirty || transform.IsRotationDirty) {
            pxBody.GlobalPose = new PxTransform(
                transform.IsRotationDirty ? transform.Rotation.ToSystem() : pxTransform.Quaternion,
                transform.IsPositionDirty ? transform.Position.ToSystem() : pxTransform.Position
            );
        }

        if (rigidBody.Type == RigidBodyComponent.BodyType.Dynamic && pxBody is PxRigidDynamic pxDynamic) {
            // pxDynamic.SetForceAndTorque(rigidBody.AccumulatedAccelerationForce.ToSystem(), rigidBody.AccumulatedAccelerationTorque.ToSystem(), PxForceMode.Acceleration);
            // pxDynamic.SetForceAndTorque(rigidBody.AccumulatedForceForce.ToSystem(), rigidBody.AccumulatedForceForce.ToSystem(), PxForceMode.Force);
            // pxDynamic.SetForceAndTorque(rigidBody.AccumulatedImpulseForce.ToSystem(), rigidBody.AccumulatedImpulseTorque.ToSystem(), PxForceMode.Impulse);
            // pxDynamic.SetForceAndTorque(rigidBody.AccumulatedVelocityChangeForce.ToSystem(), rigidBody.AccumulatedVelocityChangeTorque.ToSystem(), PxForceMode.VelocityChange);

            if (rigidBody.MaxLinearVelocity != -1f) {
                pxDynamic.MaxLinearVelocity = rigidBody.MaxLinearVelocity;
            }

            pxDynamic.AddForce(rigidBody.AccumulatedAccelerationForce.ToSystem(), PxForceMode.Acceleration);
            pxDynamic.AddForce(rigidBody.AccumulatedForceForce.ToSystem(), PxForceMode.Force);
            pxDynamic.AddForce(rigidBody.AccumulatedImpulseForce.ToSystem(), PxForceMode.Impulse);
            pxDynamic.AddForce(rigidBody.AccumulatedVelocityChangeForce.ToSystem(), PxForceMode.VelocityChange);

            pxDynamic.AddTorque(rigidBody.AccumulatedAccelerationTorque.ToSystem(), PxForceMode.Acceleration);
            pxDynamic.AddTorque(rigidBody.AccumulatedForceTorque.ToSystem(), PxForceMode.Force);
            pxDynamic.AddTorque(rigidBody.AccumulatedImpulseTorque.ToSystem(), PxForceMode.Impulse);
            pxDynamic.AddTorque(rigidBody.AccumulatedVelocityChangeTorque.ToSystem(), PxForceMode.VelocityChange);

            if (rigidBody.IsInertiaTensorDirty) {
                pxDynamic.MassSpaceInertiaTensor = rigidBody.InertiaTensor.ToSystem();
            }
        }

        transform.ClearDirtyFlags();
        rigidBody.ClearDirtyFlags();
        rigidBody.ClearForces();

        pxBody.SetActorFlag(PxActorFlag.DisableGravity, !rigidBody.IsGravityEnabled);*/
    }

    #endregion
}
