using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Ui.Components;
using Duck.Ui.Content.ContentLoader;
using Duck.Ui.RmlUi;
using Duck.Ui.Scripting;

namespace Duck.Ui.Systems;

public class UserInterfaceRenderSystem : SystemBase
{
    private readonly IFilter<UserInterfaceComponent> _filter;
    private readonly IContentModule _contentModule;
    private readonly UiModule _uiModule;

    public UserInterfaceRenderSystem(IWorld world, IContentModule contentModule, UiModule uiModule)
    {
        _contentModule = contentModule;
        _uiModule = uiModule;

        _filter = Filter<UserInterfaceComponent>(world)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityList) {

            var cmp = _filter.Get(entityId);

            if (cmp.Interface == null) {
                continue;
            }

            var ui = _uiModule.GetUserInterface(cmp.Interface);
            ui?.Context.Render();
        }
    }
}
