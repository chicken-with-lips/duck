using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using ChickenWithLips.PhysX;
using Duck.Physics.Components;
using Duck.Physics.Events;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public partial class JointSystem : BaseSystem<World, float>
{
    private readonly IPhysicsModule _physicsModule;
    private readonly IPhysicsWorld _physicsWorld;

    #region Methods

    public JointSystem(World world, IPhysicsModule physicsModule)
        : base(world)
    {
        _physicsModule = physicsModule;
        _physicsWorld = _physicsModule.GetOrCreatePhysicsWorld(world);
    }

    [Query]
    [All<PhysXIntegrationComponent>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CreateFixedJoint(in Entity entity, ref SpringJointComponent cmp)
    {
        if (!cmp.IsTargetDirty) {
            return;
        }

        if (cmp.Target.IsAlive() && cmp.Target.Entity.Has<PhysXIntegrationComponent>()) {
            var actor0 = _physicsWorld.GetRigidBody(entity);
            var actor1 = _physicsWorld.GetRigidBody(cmp.Target);

            var joint = _physicsWorld.Simulation.CreateDistanceJoint(
                actor0,
                new PxTransform(),
                actor1,
                new PxTransform(Quaternion.Identity, new Vector3D<float>(0, 10000, 0).ToSystem())
            );
            joint.MinDistance = 500f;
            joint.MaxDistance = 500f;
            joint.SetDistanceJointFlag(PxDistanceJointFlag.MaxDistanceEnabled, true);
            joint.SetDistanceJointFlag(PxDistanceJointFlag.MinDistanceEnabled, true);

            cmp.ClearDirtyFlags();
        }
    }

    #endregion
}
