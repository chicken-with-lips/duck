using BepuUtilities;
using BepuUtilities.Memory;
using Duck.Ecs;
using Duck.Logging;
using Duck.Physics.Events;
using Duck.ServiceBus;

namespace Duck.Physics;

public class PhysicsModule : IPhysicsModule, IPostTickableModule
{
    #region Members

    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    private readonly BufferPool _bufferPool = new();
    private readonly IThreadDispatcher _threadDispatcher;
    private readonly Dictionary<IWorld, IPhysicsWorld> _physicsWorlds = new();

    private float _timeAccumulator;

    #endregion

    #region Methods

    public PhysicsModule(ILogModule logModule, IEventBus eventBus)
    {
        _eventBus = eventBus;

        _logger = logModule.CreateLogger("Physics");

        // this uses defaults from the demo
        var targetThreadCount = System.Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        _threadDispatcher = new ThreadDispatcher(targetThreadCount);

        _logger.LogInformation("Initialized physics module.");
        _logger.LogInformation("Thread count: {0}", targetThreadCount);
    }

    public void PostTick()
    {
        _timeAccumulator += Time.DeltaFrame;
        var targetTimestepDuration = 1 / 60f;

        while (_timeAccumulator >= targetTimestepDuration) {
            foreach (var world in _physicsWorlds.Values) {
                world.Step(targetTimestepDuration);
            }

            _timeAccumulator -= targetTimestepDuration;
        }
    }

    public IPhysicsWorld GetOrCreatePhysicsWorld(IWorld world)
    {
        if (_physicsWorlds.TryGetValue(world, out var p)) {
            return p;
        }

        IPhysicsWorld physicsWorld = new PhysicsWorld(world, _bufferPool, _threadDispatcher);

        _physicsWorlds.Add(world, physicsWorld);

        _eventBus.Enqueue(new PhysicsWorldWasCreated {
            World = physicsWorld,
        });

        return physicsWorld;
    }

    #endregion
}
