using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Content;

namespace Duck.Ui.Systems;

public partial class UserInterfaceRenderSystem : BaseSystem<World, float>
{
    // private readonly IFilter<UserInterfaceComponent> _filter;
    private readonly IContentModule _contentModule;
    private readonly UiModule _uiModule;

    public UserInterfaceRenderSystem(World world, IContentModule contentModule, UiModule uiModule)
        : base(world)
    {
        _contentModule = contentModule;
        _uiModule = uiModule;

        // _filter = Filter<UserInterfaceComponent>(world)
        // .Build();
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run()
    {
        Console.WriteLine("TODO: UserInterfaceRenderSystem");
        // foreach (var entityId in _filter.EntityList) {
        //     var cmp = _filter.Get(entityId);
        //
        //     if (cmp.Interface == null) {
        //         continue;
        //     }
        //
        //     var ui = _uiModule.GetUserInterface(cmp.Interface);
        //     ui?.Context.Render();
        // }
    }
}
