using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuUtilities.Memory;
using Duck.Contracts;
using Duck.Contracts.Logging;
using Duck.Contracts.Physics;
using Duck.Contracts.ServiceBus;
using Duck.Ecs;
using Duck.Ecs.Events;

namespace Duck.Physics
{
    public class PhysicsSubsystem : IPhysicsSubsystem, IApplicationInitializableSubsystem, IApplicationPostTickableSubsystem
    {
        #region Members

        private readonly IEventBus _eventBus;
        private readonly ILogSubsystem _logSubsystem;
        private ILogger _logger;
        private bool _isInitialized;

        private readonly BufferPool _bufferPool = new();
        private SimpleThreadDispatcher _threadDispatcher;
        private float _timeAccumulator;

        private readonly Dictionary<IWorld, IPhysicsWorld> _physicsWorlds = new();

        #endregion

        #region Methods

        public PhysicsSubsystem(ILogSubsystem logSubsystem, IEventBus eventBus)
        {
            _logSubsystem = logSubsystem;
            _eventBus = eventBus;
        }

        public bool Init()
        {
            if (_isInitialized) {
                throw new Exception("LogSubsystem has already been initialized");
            }

            _logger = _logSubsystem.CreateLogger("Physics");

            // this uses defaults from the demo
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            _threadDispatcher = new SimpleThreadDispatcher(targetThreadCount);

            _eventBus.AddListener(new EventListener<WorldWasCreated>(OnEcsWorldWasCreated));

            _logger.LogInformation("Initialized physics subsystem.");
            _logger.LogInformation("Thread count: {0}", targetThreadCount);

            _isInitialized = true;

            return true;
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

        public IPhysicsWorld? GetPhysicsWorld(IWorld world)
        {
            if (_physicsWorlds.TryGetValue(world, out var p)) {
                return p;
            }

            return null;
        }

        private void OnEcsWorldWasCreated(WorldWasCreated ev)
        {
            _physicsWorlds.Add(ev.World, new PhysicsWorld(_bufferPool, _threadDispatcher));
        }

        #endregion
    }
}
