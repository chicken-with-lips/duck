using Arch.Core;
using Arch.System;
using Duck.Ui.Components;

namespace Duck.Ui.Systems;

public partial class ContextLoadSystem : BaseSystem<World, float>
{
    private readonly UiModule _uiModule;

    public ContextLoadSystem(World world, UiModule uiModule)
        : base(world)
    {
        _uiModule = uiModule;

        world.SubscribeComponentAdded(
            (in Entity entity, ref ContextComponent c) =>_uiModule.GetOrCreateContext(c.Name)
        ); 
    }
}
