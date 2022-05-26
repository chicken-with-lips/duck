using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public abstract class RigidBodyLifecycleSystem : SystemBase
{
    #region Methods

    protected PxRigidActor CreateBody(IEntity entity, PhysicsWorld world, PxPhysics physics, ref PhysXIntegrationComponent physxComponent, PxGeometry geometry, RigidBodyComponent rigidBodyComponent, TransformComponent transformComponent)
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

        // TODO: integrate materials in to asset system
        var material = physics.CreateMaterial(0.5f, 0.5f, 0.1f);

        // TODO: share shapes
        var shape = physics.CreateShape(geometry, material);

        if (!physxComponent.Body.AttachShape(shape)) {
            throw new Exception("FIXME: errors");
        }

        world.MapActorToEntity(entity, physxComponent.Body);

        return physxComponent.Body;
    }

    #endregion
}
