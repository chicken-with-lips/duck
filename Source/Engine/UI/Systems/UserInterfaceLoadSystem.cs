using Arch.Core;
using Arch.System;
using Duck.Content;
using Duck.Ui.Assets;
using Duck.Ui.Components;
using Duck.Ui.Content.ContentLoader;
using Duck.Ui.RmlUi;
using Duck.Ui.Scripting;

namespace Duck.Ui.Systems;

public partial class UserInterfaceLoadSystem : BaseSystem<World, float>
{
    private readonly IContentModule _contentModule;
    private readonly UiModule _uiModule;

    public UserInterfaceLoadSystem(World world, IContentModule contentModule, UiModule uiModule)
        : base(world)
    {
        _contentModule = contentModule;
        _uiModule = uiModule;

        world.SubscribeComponentAdded(
            (in Entity entity, ref UserInterfaceComponent cmp) => {
                if (cmp.Interface == null) {
                    return;
                }

                var context = _uiModule.GetOrCreateContext(cmp.ContextName);
                var ui = (RmlUserInterface)_contentModule.LoadImmediate<UserInterface>(
                    cmp.Interface,
                    new UserInterfaceLoadContext() {
                        RmlContext = context
                    }
                );

                _uiModule.RegisterUserInterface(cmp.Interface, ui);

                if (cmp.Script is IUserInterfaceLoaded loaded) {
                    loaded.OnLoaded(ui);
                }
            }
        );
    }
}
