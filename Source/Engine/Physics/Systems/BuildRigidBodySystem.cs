using System.Runtime.CompilerServices;
using ADyn.Components;
using ADyn.Shapes;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Graphics.Components;
using Duck.Physics.Components;

namespace Duck.Physics.Systems;

public partial class BuildRigidBodySystem : BaseSystem<World, float>
{
    private readonly IPhysicsModule _physicsModule;

    #region Methods

    public BuildRigidBodySystem(World world, IPhysicsModule physicsModule)
        : base(world)
    {
        _physicsModule = physicsModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BuildBoxShape(in Entity entity, in RigidBodyBuilder<BoxShape> container)
    {
        var scale = AVector3.One;
        var scaledDefinition = container.Definition;

        if (World.Has<Scale>(entity)) {
            scale = World.Get<Scale>(entity).Value;
        }

        if (scaledDefinition.Shape.HasValue) {
            scaledDefinition.Shape = scaledDefinition.Shape.Value with {
                HalfExtents = container.Definition.Shape!.Value.HalfExtents * scale,
            };
        }

        var scene = _physicsModule.GetOrCreatePhysicsScene(World) as PhysicsScene;
        scene?.Simulation.CreateRigidBody(entity, scaledDefinition);

        World.Remove<RigidBodyBuilder<BoxShape>>(entity);
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BuildSphereShape(in Entity entity, in RigidBodyBuilder<SphereShape> container)
    {
        var scale = AVector3.One;
        var scaledDefinition = container.Definition;

        if (World.Has<Scale>(entity)) {
            scale = World.Get<Scale>(entity).Value;
        }

        if (scaledDefinition.Shape.HasValue) {
            scaledDefinition.Shape = scaledDefinition.Shape.Value with {
                Radius = container.Definition.Shape!.Value.Radius * scale.X,
            };
        }

        var scene = _physicsModule.GetOrCreatePhysicsScene(World) as PhysicsScene;
        scene?.Simulation.CreateRigidBody(entity, scaledDefinition);

        World.Remove<RigidBodyBuilder<SphereShape>>(entity);
    }

    #endregion
}
