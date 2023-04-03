using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Graphics.Components;
using Duck.Physics.Components;

namespace Duck.Physics.Systems;

public partial class RigidBodyLifecycleSystem_AddSphere : BaseSystem<World, float>
{
    #region Members

    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_AddSphere(World world, IPhysicsModule physicsModule)
        : base(world)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);
    }

    [Query]
    [All<RigidBodyComponent>]
    [None<PhysXIntegrationComponent>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, in RigidBodyComponent rigidBody, in TransformComponent transform, in BoundingSphereComponent boundingSphere)
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

public partial class RigidBodyLifecycleSystem_RemoveSphere : BaseSystem<World, float>
{
    #region Members

    // private readonly IFilter<BoundingSphereComponent, PhysXIntegrationComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_RemoveSphere(World world, IPhysicsModule physicsModule)
        : base(world)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        // _filter = Filter<BoundingSphereComponent, PhysXIntegrationComponent>(world)
        // .Build();
    }

    [Query]
    public void Run()
    {
        Console.WriteLine("TODO: RigidBodyLifecycleSystem_RemoveSphere");
        // foreach (var entityId in _filter.EntityRemovedList) {
        // RemoveBody(ref _filter.Get2(entityId), _physicsWorld);

        // entity.Remove<PhysXIntegrationComponent>();
        // }
    }

    #endregion
}
