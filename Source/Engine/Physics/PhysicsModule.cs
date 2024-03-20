using Arch.Core;
using Duck.Logging;
using Duck.Physics.Events;
using Duck.Platform;
using Duck.ServiceBus;

namespace Duck.Physics;

public class PhysicsModule : IPhysicsModule, IPreTickableModule, IPostTickableModule
{
    #region Members

    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    private readonly Dictionary<World, IPhysicsScene> _physicsWorlds = new();

    private float _timeAccumulator;

    #endregion

    #region Methods

    public PhysicsModule(ILogModule logModule, IEventBus eventBus)
    {
        _eventBus = eventBus;

        _logger = logModule.CreateLogger("Physics");

        var targetThreadCount = (uint)System.Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);

        _logger.LogInformation("Created physics module.");
        Console.WriteLine("TODO: _logger.LogInformation(\"Thread count: {0}\", _cpuDispatcher.WorkerCount);");
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

    public IPhysicsScene GetOrCreatePhysicsScene(World world)
    {
        if (_physicsWorlds.TryGetValue(world, out var p)) {
            return p;
        }

        IPhysicsScene physicsScene = new PhysicsScene(world);

        _physicsWorlds.Add(world, physicsScene);

        _eventBus.Emit(
            new PhysicsWorldWasCreated(physicsScene)
        );

        return physicsScene;
    }

    public void DestroyPhysicsSceneForWorld(World world)
    {
        if (!_physicsWorlds.TryGetValue(world, out var physicsScene)) {
            return;
        }

        physicsScene.Dispose();
        _physicsWorlds.Remove(world);
    }

    #endregion
}
