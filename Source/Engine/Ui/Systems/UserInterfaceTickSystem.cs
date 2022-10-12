using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Ui.Components;
using Duck.Ui.Scripting;

namespace Duck.Ui.Systems;

public class UserInterfaceTickSystem : SystemBase
{
    private readonly IFilter<UserInterfaceComponent> _filter;

    public UserInterfaceTickSystem(IWorld world)
    {
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

            if (cmp.Script is IUserInterfaceTick tick) {
                tick.OnTick();
            }
        }
    }
}
