using System.Collections.Generic;
using Duck.Contracts;
using Duck.Contracts.SceneManagement;
using MessagePack;

namespace Duck.Ecs.Systems
{
    public class SystemComposition : ISystemComposition
    {
        public IWorld World {
            get;

            // FIXME: used to restore the World reference after a reload. serialization library doesn't handle circular
            // references
            internal set;
        }

        private readonly List<IInitSystem> _initSystemList = new();
        private readonly List<IRunSystem> _runSystemList = new();

        public SystemComposition(IWorld world)
        {
            World = world;
        }

        public ISystemComposition Add(ISystem system)
        {
            if (system is IInitSystem initSystem) {
                _initSystemList.Add(initSystem);
            }

            if (system is IRunSystem runSystem) {
                _runSystemList.Add(runSystem);
            }

            return this;
        }

        public void Init(IScene scene, IApplication app)
        {
            foreach (var system in _initSystemList) {
                system.Init(World, scene, app);
            }

            World.InitFilters();
        }

        public void Tick()
        {
            foreach (var system in _runSystemList) {
                system.Run();
            }
        }
    }
}
