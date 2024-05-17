using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.AI.Components;
using Duck.Graphics.Components;
using Duck.Physics.Components;
using Silk.NET.Maths;
using MathF = Duck.Math.MathF;

namespace Duck.AI.Systems;

public partial class AgentSteeringSystem : BaseSystem<World, float>
{
    public AgentSteeringSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetSteeringForce(ref AgentComponent agentComponent)
    {
        agentComponent.SteeringForce = Vector3D<float>.Zero;
    }

    // [Query]
    // [All<AgentSeekBehaviourComponent>]
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // private void Seek(ref AgentComponent agent, in AgentTargetComponent target, in TransformComponent transform, in RigidBodyComponent rigidBody)
    // {
    //     agent.SteeringForce += SeekToPoint(transform.Position, rigidBody.MaxLinearVelocity, target.Point);
    // }
    //
    // [Query]
    // [All<AgentPursuitBehaviourComponent>]
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // private void Pursuit(ref AgentComponent agent, in AgentTargetComponent target, in TransformComponent transform, in RigidBodyComponent rigidBody)
    // {
    //     var directionToEvader = target.Point - transform.Position;
    //     var agentHeading = transform.Forward;
    //     var relativeHeading = Vector3D.Dot(agentHeading, target.Heading);
    //
    //     if (Vector3D.Dot(directionToEvader, agentHeading) > 0 && relativeHeading < -0.95f) {
    //         Seek(ref agent, target, transform, rigidBody);
    //         return;
    //     }
    //
    //     var lookAheadTime = directionToEvader.Length / (rigidBody.MaxLinearVelocity + target.Velocity.Length);
    //
    //     agent.SteeringForce += SeekToPoint(transform.Position, rigidBody.MaxLinearVelocity, target.Point + target.Velocity * lookAheadTime);
    // }
    //
    // [Query]
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // private void ApplySteeringForce(in AgentComponent agent, in MassComponent mass, ref RigidBodyComponent rigidBody)
    // {
    //     var steeringForce = agent.SteeringForce;
    //     // var acceleration = steeringForce / mass.Value;
    //     var acceleration = steeringForce * mass.ForceMultiplier;
    //     Console.WriteLine(acceleration);
    //     rigidBody.AddForce(acceleration, RigidBodyComponent.ForceMode.Acceleration);
    // }
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // private Vector3D<float> SeekToPoint(in Vector3D<float> agentPosition, in float agentMaxSpeed, in Vector3D<float> target)
    // {
    //     return Vector3D.Normalize(agentPosition - target) * agentMaxSpeed;
    // }
}
