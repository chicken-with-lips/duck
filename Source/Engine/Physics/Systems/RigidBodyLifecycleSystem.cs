using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Physics.Components;
using Duck.Renderer.Components;

namespace Duck.Physics.Systems;

public partial class RigidBodyLifecycleSystem : BaseSystem<World, float>
{
    #region Members

    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem(World world, IPhysicsModule physicsModule)
        : base(world)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        world.SubscribeEntityDestroyed(
            RemoveBody
        );

        world.SubscribeComponentRemoved(
            ((in Entity entity, ref PhysXIntegrationComponent cmp) => RemoveBody(entity))
        );
    }

    [Query]
    [None<PhysXIntegrationComponent>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBox(in Entity entity, in RigidBodyComponent rigidBody, in TransformComponent transform, in BoundingBoxComponent boundingBox)
    {
        var physics = _physicsWorld.Physics;

        entity.Add<PhysXIntegrationComponent>();

        var geometry = PhysXHelper.CreateBoxGeometry(
            boundingBox.Box,
            transform.Scale
        );

        RigidBodyHelper.CreateBody(
            entity,
            _physicsWorld,
            physics,
            geometry,
            rigidBody,
            transform,
            ref entity.Get<PhysXIntegrationComponent>()
        );
    }

    private void RemoveBody(in Entity entity)
    {
        if (!entity.Has<PhysXIntegrationComponent>()) {
            return;
        }

        ref var cmp = ref entity.Get<PhysXIntegrationComponent>();

        RigidBodyHelper.RemoveBody(ref cmp, _physicsWorld);
    }

    [Query]
    [None<PhysXIntegrationComponent>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSphere(in Entity entity, in RigidBodyComponent rigidBody, in TransformComponent transform, in BoundingSphereComponent boundingSphere)
    {
        var physics = _physicsWorld.Physics;

        entity.Add<PhysXIntegrationComponent>();

        RigidBodyHelper.CreateBody(
            entity,
            _physicsWorld,
            physics,
            PhysXHelper.CreateSphereGeometry(boundingSphere, transform.Scale),
            rigidBody,
            transform,
            ref entity.Get<PhysXIntegrationComponent>()
        );
    }

    #endregion
}
