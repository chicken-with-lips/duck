using System.Buffers;
using Duck.Ecs.Events;
using Duck.Ecs.Serialization;
using Duck.Logging;
using Duck.ServiceBus;

namespace Duck.Ecs;

public class WorldSubsystem : IWorldSubsystem, IApplicationPreTickableSubsystem
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

    public WorldSubsystem(ILogSubsystem logSubsystem, IEventBus eventBus)
    {
        _eventBus = eventBus;

        _logger = logSubsystem.CreateLogger("Ecs");
        _logger.LogInformation("Initialized ECS world subsystem.");
    }

    public void PreTick()
    {
        foreach (var world in _worlds) {
            world.BeginFrame();
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

    public void Serialize(IWorld world, IBufferWriter<byte> destination)
    {
        new WorldSerializer().Serialize(world, destination);
    }

    public IWorld Deserialize(ReadOnlyMemory<byte> data)
    {
        return new WorldSerializer().Deserialize(data);
    }

    #endregion
}
