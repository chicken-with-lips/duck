using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using ChickenWithLips.PhysX;
using Duck.Physics.Components;
using Duck.Renderer.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public partial class RigidBodySynchronizationSystem : BaseSystem<World, float>
{
    #region Methods

    public RigidBodySynchronizationSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(ref RigidBodyComponent rigidBody, in PhysXIntegrationComponent physXIntegration, ref TransformComponent transform)
    {
        var pxTransform = physXIntegration.Body.GlobalPose;

        var body = physXIntegration.Body;

        if (transform.IsPositionDirty || transform.IsRotationDirty) {
            body.GlobalPose = new PxTransform(
                transform.IsRotationDirty ? transform.Rotation.ToSystem() : pxTransform.Quaternion,
                transform.IsPositionDirty ? transform.Position.ToSystem() : pxTransform.Position
            );
        } else {
            transform.Position = pxTransform.Position.ToGeneric();
            transform.Rotation = pxTransform.Quaternion.ToGeneric();
        }

        transform.ClearDirtyFlags();

        if (rigidBody.Type == RigidBodyComponent.BodyType.Dynamic && body is PxRigidDynamic dynamic) {
            dynamic.SetForceAndTorque(rigidBody.AccumulatedAccelerationForce.ToSystem(), rigidBody.AccumulatedAccelerationTorque.ToSystem(), PxForceMode.Acceleration);
            dynamic.SetForceAndTorque(rigidBody.AccumulatedForceForce.ToSystem(), rigidBody.AccumulatedForceForce.ToSystem(), PxForceMode.Force);
            dynamic.SetForceAndTorque(rigidBody.AccumulatedImpulseForce.ToSystem(), rigidBody.AccumulatedImpulseTorque.ToSystem(), PxForceMode.Impulse);
            dynamic.SetForceAndTorque(rigidBody.AccumulatedVelocityChangeForce.ToSystem(), rigidBody.AccumulatedVelocityChangeTorque.ToSystem(), PxForceMode.VelocityChange);

            // dynamic.AddForce(rigidBody.AccumulatedAccelerationForce.ToSystem(), PxForceMode.Acceleration);
            // dynamic.AddForce(rigidBody.AccumulatedForceForce.ToSystem(), PxForceMode.Force);
            // dynamic.AddForce(rigidBody.AccumulatedImpulseForce.ToSystem(), PxForceMode.Impulse);
            // dynamic.AddForce(rigidBody.AccumulatedVelocityChangeForce.ToSystem(), PxForceMode.VelocityChange);
            //
            // dynamic.AddTorque(rigidBody.AccumulatedAccelerationTorque.ToSystem(), PxForceMode.Acceleration);
            // dynamic.AddTorque(rigidBody.AccumulatedForceTorque.ToSystem(), PxForceMode.Force);
            // dynamic.AddTorque(rigidBody.AccumulatedImpulseTorque.ToSystem(), PxForceMode.Impulse);
            // dynamic.AddTorque(rigidBody.AccumulatedVelocityChangeTorque.ToSystem(), PxForceMode.VelocityChange);

            rigidBody.LinearVelocity = dynamic.LinearVelocity.ToGeneric();
            rigidBody.AngularVelocity = dynamic.AngularVelocity.ToGeneric();

            if (rigidBody.IsInertiaTensorDirty) {
                dynamic.MassSpaceInertiaTensor = rigidBody.InertiaTensor.ToSystem();
            } else {
                rigidBody.InertiaTensor = dynamic.MassSpaceInertiaTensor.ToGeneric();
            }

            rigidBody.MassSpaceInvInertiaTensor = dynamic.MassSpaceInvInertiaTensor.ToGeneric();
            rigidBody.InertiaTensorRotation = dynamic.CenterMassLocalPose.Quaternion.ToGeneric();
        }

        rigidBody.ClearDirtyFlags();
        rigidBody.ClearForces();

        physXIntegration.Body.SetActorFlag(PxActorFlag.DisableGravity, !rigidBody.IsGravityEnabled);
    }

    #endregion
}
