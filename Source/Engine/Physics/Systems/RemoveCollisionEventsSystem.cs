using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using ChickenWithLips.PhysX;
using Duck.Physics.Components;
using Duck.Physics.Events;
using Duck.Graphics.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public partial class RemoveCollisionEventsSystem : BaseSystem<World, float>
{
    #region Methods

    public RemoveCollisionEventsSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, in PhysicsCollision cmp)
    {
        World.Destroy(entity);
    }

    #endregion
}
