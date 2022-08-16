using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Ui.Components;
using Duck.Ui.RmlUi;
using Duck.Ui.Scripting;

namespace Duck.Ui.Systems;

public class UserInterfaceLoadSystem : SystemBase
{
    private readonly IFilter<UserInterfaceComponent> _filter;
    private readonly IContentModule _contentModule;
    private readonly UiModule _uiModule;

    public UserInterfaceLoadSystem(IWorld world, IContentModule contentModule, UiModule uiModule)
    {
        _contentModule = contentModule;
        _uiModule = uiModule;

        _filter = Filter<UserInterfaceComponent>(world)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityAddedList) {
            var cmp = _filter.Get(entityId);

            if (cmp.Interface == null) {
                continue;
            }

            var context = _uiModule.GetContext(cmp.ContextName);
            var ui = (RmlUserInterface)_contentModule.LoadImmediate(
                cmp.Interface,
                new UserInterfaceLoadContext() {
                    RmlContext = context
                }
            );

            _uiModule.RegisterUserInterface(cmp.Interface, ui);

            if (cmp.Script is IUserInterfaceLoaded loaded) {
                loaded.OnLoaded();
            }
        }

        foreach (var entityId in _filter.EntityRemovedList) {
            Console.WriteLine("TODO: remove ui");
        }
    }
}

internal struct UserInterfaceLoadContext : IAssetLoadContext
{
    public RmlContext RmlContext;
}
