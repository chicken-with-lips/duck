using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Physics.Events;

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
