using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Graphics.Components;
using Duck.Physics.Components;

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
    public void AddBox(in Entity entity, in RigidBodyComponent rigidBody, in TransformComponent transform, in BoundingBoxComponent boundingBox, in MassComponent mass)
    {
        Console.WriteLine("TODO: RigidBodyLifecycleSystem.AddBox");
        /*var physics = _physicsWorld.Physics;

        World.Add<PhysXIntegrationComponent>(entity);

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
            mass,
            transform,
            ref World.Get<PhysXIntegrationComponent>(entity)
        );*/
    }

    private void RemoveBody(in Entity entity)
    {
        Console.WriteLine("TODO: RigidBodyLifecycleSystem.RemoveBody");
       /* if (!World.Has<PhysXIntegrationComponent>(entity)) {
            return;
        }

        ref var cmp = ref World.Get<PhysXIntegrationComponent>(entity);

        RigidBodyHelper.RemoveBody(ref cmp, _physicsWorld);*/
    }

    [Query]
    [None<PhysXIntegrationComponent>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSphere(in Entity entity, in RigidBodyComponent rigidBody, in TransformComponent transform, in BoundingSphereComponent boundingSphere, in MassComponent mass)
    {
        Console.WriteLine("TODO: RigidBodyLifecycleSystem.AddSphere");
        /*var physics = _physicsWorld.Physics;

        World.Add<PhysXIntegrationComponent>(entity);

        RigidBodyHelper.CreateBody(
            entity,
            _physicsWorld,
            physics,
            PhysXHelper.CreateSphereGeometry(boundingSphere, transform.Scale),
            rigidBody,
            mass,
            transform,
            ref World.Get<PhysXIntegrationComponent>(entity)
        );*/
    }

    #endregion
}
