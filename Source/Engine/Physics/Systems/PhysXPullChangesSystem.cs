using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using ChickenWithLips.PhysX;
using Duck.Graphics.Components;
using Duck.Physics.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public partial class PhysXPullChanges : BaseSystem<World, float>
{
    private readonly PhysicsWorld _physicsWorld;

    #region Methods

    public PhysXPullChanges(World world, PhysicsWorld physicsWorld)
        : base(world)
    {
        _physicsWorld = physicsWorld;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(ref RigidBodyComponent rigidBody, in PhysXIntegrationComponent physXIntegration, ref TransformComponent transform)
    {
        var pxBody = _physicsWorld.GetActor(physXIntegration.BodyId);
        var pxTransform = pxBody.GlobalPose;

        transform.Position = pxTransform.Position.ToGeneric();
        transform.Rotation = pxTransform.Quaternion.ToGeneric();

        if (rigidBody.Type == RigidBodyComponent.BodyType.Dynamic && pxBody is PxRigidDynamic pxDynamic) {
            rigidBody.LinearVelocity = pxDynamic.LinearVelocity.ToGeneric();
            
            rigidBody.AngularVelocity = pxDynamic.AngularVelocity.ToGeneric();

            rigidBody.InertiaTensor = pxDynamic.MassSpaceInertiaTensor.ToGeneric();
            rigidBody.MassSpaceInvInertiaTensor = pxDynamic.MassSpaceInvInertiaTensor.ToGeneric();
            rigidBody.InertiaTensorRotation = pxDynamic.CenterMassLocalPose.Quaternion.ToGeneric();
        }
    }

    #endregion
}
