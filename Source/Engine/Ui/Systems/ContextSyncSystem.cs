using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Ui.Components;

namespace Duck.Ui.Systems;

public class ContextSyncSystem : SystemBase
{
    private readonly IFilter<ContextComponent> _filter;
    private readonly UiModule _uiModule;
    private readonly IWorld _world;

    public ContextSyncSystem(IWorld world, UiModule uiModule)
    {
        _world = world;
        _uiModule = uiModule;

        _filter = Filter<ContextComponent>(_world)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityList) {
            var cmp = _filter.Get(entityId);
            var context = _uiModule.GetOrCreateContext(cmp.Name);

            if (null != context) {
                context.ShouldReceiveInput = cmp.ShouldReceiveInput;
                context.Tick();
            }
        }
    }
}
