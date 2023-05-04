using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Ui.Components;
using Duck.Ui.Scripting;

namespace Duck.Ui.Systems;

public partial class UserInterfaceTickSystem : BaseSystem<World, float>
{
    public UserInterfaceTickSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in UserInterfaceComponent cmp)
    {
        if (cmp.Interface == null) {
            return;
        }

        if (cmp.Script is IUserInterfaceTick tick) {
            tick.OnTick();
        }
    }
}
