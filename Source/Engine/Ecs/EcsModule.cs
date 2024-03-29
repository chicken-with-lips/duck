using System.Buffers;
using Duck.Ecs.Events;
using Duck.Ecs.Serialization;
using Duck.Logging;
using Duck.ServiceBus;

namespace Duck.Ecs;

public class EcsModule : IEcsModule, IPreTickableModule, ITickableModule, IPostTickableModule
{
    #region Properties

    public IWorld[] Worlds => _worlds.ToArray();

    #endregion

    #region Members

    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly List<IWorld> _worlds = new();

    #endregion

    #region Methods

    public EcsModule(ILogModule logModule, IEventBus eventBus)
    {
        _eventBus = eventBus;

        _logger = logModule.CreateLogger("Ecs");
        _logger.LogInformation("Created ECS world module.");
    }

    public void PreTick()
    {
        foreach (var world in _worlds) {
            world.BeginFrame();
        }
    }

    public void Tick()
    {
        foreach (var world in _worlds) {
            world.Tick();
        }
    }

    public void PostTick()
    {
        foreach (var world in _worlds) {
            world.EndFrame();
        }
    }

    public IWorld Create()
    {
        var world = new World();

        _worlds.Add(world);

        _eventBus.Enqueue(new WorldWasCreated() {
            World = world,
        });

        return world;
    }

    public void Destroy(IWorld world)
    {
        _worlds.Remove(world);
        world.Dispose();
    }

    #endregion
}
