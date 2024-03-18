﻿using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.AI.Components;
using Duck.Graphics.Components;
using Duck.Physics.Components;

namespace Duck.AI.Systems;

public partial class AgentTargetAtEntitySystem : BaseSystem<World, float>
{
    public AgentTargetAtEntitySystem(World world)
        : base(world)
    {
    }

    [Query]
    [All<AgentComponent>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Run(ref AgentTargetComponent target, in AgentTargetEntityComponent targetEntity)
    {
        if (!targetEntity.Value.HasValue || !World.IsAlive(targetEntity.Value.Value)) {
            return;
        }

        if (!World.Has<TransformComponent>(targetEntity.Value.Value)
            || !World.Has<RigidBodyComponent>(targetEntity.Value.Value)) {
            return;
        }

        var targetTransform = World.Get<TransformComponent>(targetEntity.Value.Value);
        var targetRigidBody = World.Get<RigidBodyComponent>(targetEntity.Value.Value);

        target.Point = targetTransform.Position;
        target.Heading = targetTransform.Forward;
        target.Velocity = targetRigidBody.LinearVelocity;
    }
}
