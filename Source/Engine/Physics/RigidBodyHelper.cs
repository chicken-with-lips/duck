using Arch.Core;
using ChickenWithLips.PhysX;
using Duck.Physics.Components;
using Duck.Renderer.Components;
using Silk.NET.Maths;

namespace Duck.Physics;

internal static class RigidBodyHelper
{
    #region Methods

    public static PxRigidActor CreateBody(in Entity entity, PhysicsWorld world, PxPhysics physics, PxGeometry geometry, in RigidBodyComponent rigidBodyComponent, in TransformComponent transformComponent, ref PhysXIntegrationComponent physxComponent)
    {
        var position = transformComponent.Position.ToSystem();
        var rotation = transformComponent.Rotation.ToSystem();

        if (rigidBodyComponent.Type == RigidBodyComponent.BodyType.Dynamic || rigidBodyComponent.Type == RigidBodyComponent.BodyType.Kinematic) {
            var body = physics.CreateRigidDynamic(
                new PxTransform(rotation, position)
            );

            PxRigidBodyExt.UpdateMassAndInertia(body, rigidBodyComponent.Mass);

            if (rigidBodyComponent.Type == RigidBodyComponent.BodyType.Kinematic) {
                body.SetFlag(PxRigidBodyFlag.Kinematic, true);
            }

            body.LockFlags = (PxRigidDynamicLockFlag)rigidBodyComponent.AxisLock;
            body.AngularDamping = rigidBodyComponent.AngularDamping;
            body.LinearDamping = rigidBodyComponent.LinearDamping;

            physxComponent.Body = body;
        } else if (rigidBodyComponent.Type == RigidBodyComponent.BodyType.Static) {
            var body = physics.CreateRigidStatic(
                new PxTransform(rotation, position)
            );

            physxComponent.Body = body;
        }

        // FIXME: integrate materials in to asset system
        var material = physics.CreateMaterial(0.5f, 0.5f, 0.1f);

        // FIXME: share shapes
        var shape = physics.CreateShape(geometry, material);

        if (!physxComponent.Body.AttachShape(shape)) {
            throw new Exception("FIXME: errors");
        }

        world.MapActorToEntity(entity, physxComponent.Body);
        world.Scene.AddActor(physxComponent.Body);

        return physxComponent.Body;
    }

    public static void RemoveBody(ref PhysXIntegrationComponent physxComponent, PhysicsWorld world)
    {
        world.UnmapActor(physxComponent.Body);
        world.Scene.RemoveActor(physxComponent.Body);
    }
    
    #endregion
}
