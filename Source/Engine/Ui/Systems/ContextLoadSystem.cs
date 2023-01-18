using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Ui.Components;

namespace Duck.Ui.Systems;

public class ContextLoadSystem : SystemBase
{
    private readonly IFilter<ContextComponent> _filter;
    private readonly UiModule _uiModule;
    private readonly IWorld _world;

    public ContextLoadSystem(IWorld world, UiModule uiModule)
    {
        _world = world;
        _uiModule = uiModule;

        _filter = Filter<ContextComponent>(_world)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityAddedList) {
            var cmp = _filter.Get(entityId);

            _uiModule.GetOrCreateContext(cmp.Name);
        }

        foreach (var entityId in _filter.EntityRemovedList) {
            Console.WriteLine("TODO: remove ui context");
        }
    }
}
