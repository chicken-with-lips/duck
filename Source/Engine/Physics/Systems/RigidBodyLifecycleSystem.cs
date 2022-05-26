using System.Numerics;
using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class RigidBodyLifecycleSystem : SystemBase
{
    #region Members

    private readonly IFilter<RigidBodyComponent, TransformComponent, BoundingBoxComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<RigidBodyComponent, TransformComponent, BoundingBoxComponent>(world)
            .Build();
    }

    public override void Run()
    {
        var physics = _physicsWorld.Physics;
        var scene = _physicsWorld.Scene;

        foreach (var entityId in _filter.EntityAddedList) {
            IEntity entity = _filter.GetEntity(entityId);
            ref RigidBodyComponent rigidBodyComponent = ref _filter.Get1(entityId);
            ref PhysXIntegrationComponent physxComponent = ref entity.Get<PhysXIntegrationComponent>();

            TransformComponent transformComponent = _filter.Get2(entityId);
            BoundingBoxComponent boundingBoxComponent = _filter.Get3(entityId);
            Vector3D<float> scale = transformComponent.Scale;

            var geometry = CreateGeometry(rigidBodyComponent.Shape, boundingBoxComponent.Box, scale);

            var body = CreateBody(
                physics,
                ref physxComponent,
                geometry,
                rigidBodyComponent,
                transformComponent.Position.ToSystem(),
                transformComponent.Rotation.ToSystem()
            );

            scene.AddActor(body);
        }
    }

    private static PxBoxGeometry CreateGeometry(RigidBodyComponent.BodyShape shape, Box3D<float> boundingBox, Vector3D<float> scale)
    {
        if (shape == RigidBodyComponent.BodyShape.Box) {
            var x = (MathF.Abs(boundingBox.Min.X) + MathF.Abs(boundingBox.Max.X)) * scale.X;
            var y = (MathF.Abs(boundingBox.Min.Y) + MathF.Abs(boundingBox.Max.Y)) * scale.Y;
            var z = (MathF.Abs(boundingBox.Min.Z) + MathF.Abs(boundingBox.Max.Z)) * scale.Z;

            return new PxBoxGeometry(new Vector3(x / 2f, y / 2f, z / 2f));
        } else if (shape == RigidBodyComponent.BodyShape.Capsule) {
            // Capsule capsule = new Capsule(rigidBodyComponent.CapsuleSettings.Radius, rigidBodyComponent.CapsuleSettings.Length);
            // capsule.ComputeInertia(rigidBodyComponent.Mass);
        }

        throw new Exception("FIXME: errors");
    }

    private PxRigidActor CreateBody(PxPhysics physics, ref PhysXIntegrationComponent physxComponent, PxGeometry geometry, RigidBodyComponent rigidBodyComponent, Vector3 position, Quaternion rotation)
    {
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

        return physxComponent.Body;
    }

    #endregion
}
