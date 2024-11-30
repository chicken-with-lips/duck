using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Graphics.Components;
using Silk.NET.Maths;

namespace Duck.Graphics.Systems;

public partial class UpdateLocalToWorldSystem : BaseSystem<World, float>
{
    public UpdateLocalToWorldSystem(World world)
        : base(world)
    {
    }

    [Query]
    [All<LocalTransform>]
    [None<LocalToWorld>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLocalToWorldComponent(in Entity entity)
    {
        World.Add<LocalToWorld>(entity);
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateLocalToWorldComponent(in LocalTransform localTransform, ref LocalToWorld localToWorld)
    {
        localToWorld.Value = Matrix4X4.CreateScale(localTransform.Scale)
                             * Matrix4X4.CreateFromQuaternion(localTransform.Orientation)
                             * Matrix4X4.CreateTranslation(localTransform.Position);
    }
}
