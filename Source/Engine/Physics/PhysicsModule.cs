using ChickenWithLips.PhysX;
using Duck.Ecs;
using Duck.Logging;
using Duck.Physics.Events;
using Duck.ServiceBus;

namespace Duck.Physics;

public class PhysicsModule : IPhysicsModule, IPreTickableModule, IPostTickableModule
{
    #region Members

    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    private readonly PxDefaultCpuDispatcher _cpuDispatcher;
    private readonly PxFoundation _foundation;
    private readonly PxPhysics _physics;
    private readonly PxPvd _physicsDebugger;
    private readonly PxTolerancesScale _scale;

    private readonly Dictionary<IWorld, IPhysicsWorld> _physicsWorlds = new();

    private float _timeAccumulator;

    #endregion

    #region Methods

    public PhysicsModule(ILogModule logModule, IEventBus eventBus)
    {
        _eventBus = eventBus;

        _logger = logModule.CreateLogger("Physics");

        var targetThreadCount = (uint)System.Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);

        _foundation = PxFoundation.Create(PxVersion.Version);
        _cpuDispatcher = PxDefaultCpuDispatcher.Create(targetThreadCount);
        _scale = PxTolerancesScale.Default;

        var transport = PxPvdTransport.CreateDefaultSocketTransport("192.168.1.77", 5425, 10000);

        _physicsDebugger = new PxPvd(_foundation);

        // if (!_physicsDebugger.Connect(transport, PxPvdInstrumentationFlag.All)) {
            // Console.WriteLine("FIXME: could not connect to pvd");
        // }

        _physics = PxPhysics.Create(_foundation, PxVersion.Version, _scale, true, _physicsDebugger);
        _physics.InitExtensions(_physicsDebugger);

        _logger.LogInformation("Created physics module.");
        _logger.LogInformation("Thread count: {0}", _cpuDispatcher.WorkerCount);
    }

    public void PreTick()
    {
        foreach (var world in _physicsWorlds.Values) {
            world.EmitEvents(_eventBus);
        }
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

        IPhysicsWorld physicsWorld = new PhysicsWorld(world, _physics, _cpuDispatcher);

        _physicsWorlds.Add(world, physicsWorld);

        
        _eventBus.Enqueue(
            new PhysicsWorldWasCreated(physicsWorld)
        );

        return physicsWorld;
    }


    #endregion
}
