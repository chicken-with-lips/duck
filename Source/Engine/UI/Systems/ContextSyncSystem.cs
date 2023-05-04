using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Ui.Components;

namespace Duck.Ui.Systems;

public partial class ContextSyncSystem : BaseSystem<World, float>
{
    private readonly UiModule _uiModule;

    public ContextSyncSystem(World world, UiModule uiModule)
        : base(world)
    {
        _uiModule = uiModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in ContextComponent cmp)
    {
        var context = _uiModule.GetOrCreateContext(cmp.Name);
        context.ShouldReceiveInput = cmp.ShouldReceiveInput;
        context.Tick();
    }
}
