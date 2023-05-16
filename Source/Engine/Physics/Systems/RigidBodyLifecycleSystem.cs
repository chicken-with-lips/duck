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

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveBox(in PhysXIntegrationComponent integrationComponent)
    {
        // Console.WriteLine("TODO: RigidBodyLifecycleSystem_RemoveBox");
        // foreach (var entityId in _filter.EntityRemovedList) {
        // RemoveBody(ref _filter.Get2(entityId), _physicsWorld);

        // entity.Remove<PhysXIntegrationComponent>();
        // }
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

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveSphere(in PhysXIntegrationComponent integrationComponent)
    {
        // Console.WriteLine("TODO: RigidBodyLifecycleSystem_RemoveSphere");
        // foreach (var entityId in _filter.EntityRemovedList) {
        // RemoveBody(ref _filter.Get2(entityId), _physicsWorld);

        // entity.Remove<PhysXIntegrationComponent>();
        // }
    }

    #endregion
}
