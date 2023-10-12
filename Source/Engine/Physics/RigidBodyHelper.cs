using Arch.Core;
using ChickenWithLips.PhysX;
using Duck.Physics.Components;
using Duck.Graphics.Components;
using Silk.NET.Maths;

namespace Duck.Physics;

internal static class RigidBodyHelper
{
    #region Methods

    public static PxRigidActor CreateBody(in Entity entity, PhysicsWorld world, PxPhysics physics, PxGeometry geometry, in RigidBodyComponent rigidBodyComponent, in TransformComponent transformComponent, ref PhysXIntegrationComponent physxComponent)
    {
        var position = transformComponent.Position.ToSystem();
        var rotation = transformComponent.Rotation.ToSystem();
        PxRigidActor body = null;

        if (rigidBodyComponent.Type == RigidBodyComponent.BodyType.Dynamic || rigidBodyComponent.Type == RigidBodyComponent.BodyType.Kinematic) {
            var dynBody = physics.CreateRigidDynamic(
                new PxTransform(rotation, position)
            );

            PxRigidBodyExt.UpdateMassAndInertia(dynBody, rigidBodyComponent.Mass);

            if (rigidBodyComponent.Type == RigidBodyComponent.BodyType.Kinematic) {
                dynBody.SetFlag(PxRigidBodyFlag.Kinematic, true);
            }

            dynBody.LockFlags = (PxRigidDynamicLockFlag)rigidBodyComponent.AxisLock;
            dynBody.AngularDamping = rigidBodyComponent.AngularDamping;
            dynBody.LinearDamping = rigidBodyComponent.LinearDamping;

            body = dynBody;
        } else if (rigidBodyComponent.Type == RigidBodyComponent.BodyType.Static) {
            body = physics.CreateRigidStatic(
                new PxTransform(rotation, position)
            );
        }

        // FIXME: integrate materials in to asset system
        var material = physics.CreateMaterial(0.5f, 0.5f, 0.1f);

        // FIXME: share shapes
        var shape = physics.CreateShape(geometry, material);

        if (!body.AttachShape(shape)) {
            throw new Exception("FIXME: errors");
        }

        var bodyId = world.AddActor(body);
        world.MapActorToEntity(entity, bodyId);

        physxComponent.BodyId = bodyId;

        return body;
    }

    public static void RemoveBody(ref PhysXIntegrationComponent physxComponent, PhysicsWorld world)
    {
        world.UnmapActor(physxComponent.BodyId);
        world.RemoveActor(physxComponent.BodyId);
    }
    
    #endregion
}
