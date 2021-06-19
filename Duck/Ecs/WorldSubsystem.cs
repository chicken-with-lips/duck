using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Duck.Contracts;
using Duck.Contracts.Logging;
using Duck.Contracts.ServiceBus;
using Duck.Ecs.Events;
using Duck.Ecs.Serialization;
using Duck.Logging;
using Duck.Physics;

namespace Duck.Ecs
{
    public class WorldSubsystem : IWorldSubsystem, IApplicationInitializableSubsystem, IApplicationPreTickableSubsystem
    {
        #region Properties

        public IWorld[] Worlds => _worlds.ToArray();

        #endregion

        #region Members

        private readonly IEventBus _eventBus;

        private readonly ILogSubsystem _logSubsystem;
        private ILogger _logger;

        private bool _isInitialized;
        private readonly List<IWorld> _worlds = new();

        #endregion

        #region Methods

        public WorldSubsystem(ILogSubsystem logSubsystem, IEventBus eventBus)
        {
            _logSubsystem = logSubsystem;
            _eventBus = eventBus;
        }

        public bool Init()
        {
            if (_isInitialized) {
                throw new Exception("LogSubsystem has already been initialized");
            }

            _logger = _logSubsystem.CreateLogger("Ecs");
            _logger.LogInformation("Initialized ECS world subsystem.");

            _isInitialized = true;

            return true;
        }

        public void PreTick()
        {
            foreach (var world in _worlds) {
                foreach (var filter in world.Filters) {
                    filter.SwapDirtyBuffers();
                }
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
}
