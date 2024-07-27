using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
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
    [All<PhysicsCollision>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity)
    {
        World.Destroy(entity);
    }

    #endregion
}
