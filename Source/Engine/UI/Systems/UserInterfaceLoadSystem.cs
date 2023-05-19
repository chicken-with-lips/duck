using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Content;
using Duck.Ui.Assets;
using Duck.Ui.Components;
using Duck.Ui.Content.ContentLoader;
using Duck.Ui.RmlUi;
using Duck.Ui.Scripting;

namespace Duck.Ui.Systems;

// FIXME: remove when arch adds entity lifecycles
public struct UserInterfaceLoaded
{
}

public partial class UserInterfaceLoadSystem : BaseSystem<World, float>
{
    // private readonly IFilter<UserInterfaceComponent> _filter;
    private readonly IContentModule _contentModule;
    private readonly UiModule _uiModule;

    public UserInterfaceLoadSystem(World world, IContentModule contentModule, UiModule uiModule)
        : base(world)
    {
        _contentModule = contentModule;
        _uiModule = uiModule;
    }

    [Query]
    [None<UserInterfaceLoaded>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, in UserInterfaceComponent cmp)
    {
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

        entity.Add<UserInterfaceLoaded>();

        // foreach (var entityId in _filter.EntityRemovedList) {
        //     Console.WriteLine("FIXME: remove ui");
        // }
    }
}
