using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Graphics.Components;
using Duck.Physics.Components;

namespace Duck.Physics.Systems;

public partial class RigidBodyLifecycleSystem_AddBox : BaseSystem<World, float>
{
    #region Members

    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_AddBox(World world, IPhysicsModule physicsModule)
        : base(world)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);
    }

    [Query]
    [None<PhysXIntegrationComponent>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, in RigidBodyComponent rigidBody, in TransformComponent transform, in BoundingBoxComponent boundingBox)
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

    #endregion
}

public partial class RigidBodyLifecycleSystem_RemoveBox : BaseSystem<World, float>
{
    #region Members

    // private readonly IFilter<BoundingBoxComponent, PhysXIntegrationComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public RigidBodyLifecycleSystem_RemoveBox(World world, IPhysicsModule physicsModule)
        : base(world)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        // _filter = Filter<BoundingBoxComponent, PhysXIntegrationComponent>(world)
        // .Build();
    }

    [Query]
    public void Run()
    {
        // Console.WriteLine("TODO: RigidBodyLifecycleSystem_RemoveBox");
        // foreach (var entityId in _filter.EntityRemovedList) {
        // RemoveBody(ref _filter.Get2(entityId), _physicsWorld);

        // entity.Remove<PhysXIntegrationComponent>();
        // }
    }

    #endregion
}
