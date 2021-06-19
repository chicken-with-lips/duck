namespace Duck.Ecs.Systems;

public class SystemComposition : ISystemComposition
{
    public IWorld World { get; }

    private readonly List<IRunSystem> _runSystemList = new();

    public SystemComposition(IWorld world)
    {
        World = world;
    }

    public ISystemComposition Add(ISystem system)
    {
        if (system is IRunSystem runSystem) {
            _runSystemList.Add(runSystem);
        }

        return this;
    }

    public void Init()
    {
        World.InitFilters();
    }

    public void Tick()
    {
        foreach (var system in _runSystemList) {
            system.Run();
        }
    }
}
