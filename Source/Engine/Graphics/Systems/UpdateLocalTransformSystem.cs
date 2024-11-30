using System.Runtime.CompilerServices;
using ADyn.Components;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Graphics.Components;
using Transform = Duck.Math.Transform;

namespace Duck.Graphics.Systems;

public partial class UpdateLocalTransformSystem : BaseSystem<World, float>
{
    public UpdateLocalTransformSystem(World world)
        : base(world)
    {
    }

    [Query]
    [All<Position, Orientation>]
    [None<LocalTransform>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLocalTransformComponent(in Entity entity)
    {
        World.Add<LocalTransform>(entity);
    }

    [Query]
    [None<PresentPosition, PresentOrientation, Scale>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateLocalTransformWithoutScaleOrPresentationComponent(in Position position, in Orientation orientation, ref LocalTransform localTransform)
    {
        UpdateLocalTransform(position.Value, orientation.Value, AVector3.One, ref localTransform);
    }

    [Query]
    [None<PresentPosition, PresentOrientation>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateLocalTransformWithScaleButNoPresentationComponent(in Position position, in Orientation orientation, in Scale scale, ref LocalTransform localTransform)
    {
        UpdateLocalTransform(position.Value, orientation.Value, scale.Value, ref localTransform);
    }

    [Query]
    [None<Scale>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateLocalTransformWithoutScaleComponent(in PresentPosition position, in PresentOrientation orientation, ref LocalTransform localTransform)
    {
        UpdateLocalTransform(position.Value, orientation.Value, AVector3.One, ref localTransform);
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateLocalTransformWithScaleComponent(in PresentPosition position, in PresentOrientation orientation, in Scale scale, ref LocalTransform localTransform)
    {
        UpdateLocalTransform(position.Value, orientation.Value, scale.Value, ref localTransform);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateLocalTransform(in AVector3 position, in AQuaternion orientation, in AVector3 scale, ref LocalTransform localTransform)
    {
        localTransform.Position = position;
        localTransform.Orientation = orientation;
        localTransform.Scale = scale;
        localTransform.Up = Transform.Up(position, orientation);
        localTransform.Forward = Transform.Forward(position, orientation);
        localTransform.Right = Transform.Right(position, orientation);
    }
}
