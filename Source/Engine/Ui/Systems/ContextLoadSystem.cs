using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.Ui.Components;

namespace Duck.Ui.Systems;

// FIXME: remove when arch adds entity lifecycles
public struct ContextLoaded
{
}

public partial class ContextLoadSystem : BaseSystem<World, float>
{
    private readonly UiModule _uiModule;

    public ContextLoadSystem(World world, UiModule uiModule)
        : base(world)
    {
        _uiModule = uiModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, in ContextComponent cmp)
    {
        // FIXME: context load
        _uiModule.GetOrCreateContext(cmp.Name);

        entity.Add<ContextLoaded>();

        /*foreach (var entityId in _filter.EntityRemovedList) {
            Console.WriteLine("TODO: remove ui context");
        }*/
    }
}
